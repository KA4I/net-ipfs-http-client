// <copyright file="FileSystemApi.cs" company="improvGroup, LLC">
//     Copyright © 2015-2022 Richard Schneider, Marshall Rosenstein, William Forney
// </copyright>
namespace Ipfs.Http.Client.CoreApi;

using Ipfs.CoreApi;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Runtime.CompilerServices;
using System.Text;

/// <summary>
/// Class FileSystemApi.
/// Implements the <see cref="Ipfs.CoreApi.IFileSystemApi" />
/// </summary>
/// <seealso cref="Ipfs.CoreApi.IFileSystemApi" />
public class FileSystemApi : IFileSystemApi
{
    private readonly IIpfsClient ipfs;
    private readonly ILogger<FileSystemApi> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileSystemApi"/> class.
    /// </summary>
    /// <param name="ipfs">The ipfs.</param>
    /// <param name="logger">The logger.</param>
    /// <exception cref="System.ArgumentNullException">ipfs</exception>
    /// <exception cref="System.ArgumentNullException">logger</exception>
    public FileSystemApi(IIpfsClient ipfs, ILogger<FileSystemApi> logger)
    {
        this.ipfs = ipfs ?? throw new ArgumentNullException(nameof(ipfs));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public async Task<IFileSystemNode> AddAsync(Stream stream, string name = "", AddFileOptions? options = null, CancellationToken cancel = default)
    {
        options ??= new AddFileOptions();

        var opts = new List<string>();
        if (options.Pin == false)
        {
            opts.Add("pin=false");
        }

        if (options.Wrap == true)
        {
            opts.Add("wrap-with-directory=true");
        }

        if (options.RawLeaves == true)
        {
            opts.Add("raw-leaves=true");
        }

        if (options.OnlyHash == true)
        {
            opts.Add("only-hash=true");
        }

        if (options.Trickle == true)
        {
            opts.Add("trickle=true");
        }

        if (options.Progress is not null)
        {
            opts.Add("progress=true");
        }

        if (options.Hash is not null && options.Hash != MultiHash.DefaultAlgorithmName)
        {
            opts.Add($"hash={options.Hash}");
        }

        if (!string.IsNullOrWhiteSpace(options.Chunker))
        {
            opts.Add($"chunker={options.Chunker}");
        }

        var response = await this.ipfs.ExecuteCommand<Stream?, Stream?>("add", null, stream, name, cancel, opts.ToArray());

        // The result is a stream of LDJSON objects.
        // See https://github.com/ipfs/go-ipfs/issues/4852
        FileSystemNode? fsn = null;
        if (response is not null && response.CanRead)
        {
            using var sr = new StreamReader(response);
            using var jr = new JsonTextReader(sr) { SupportMultipleContent = true };
            while (await jr.ReadAsync(cancel))
            {
                var r = await JObject.LoadAsync(jr, cancel);

                // If a progress report.
                if (r?.ContainsKey("Bytes") ?? false)
                {
                    options.Progress?.Report(
                        new TransferProgress
                        {
                            Name = (string?)r?["Name"],
                            Bytes = (ulong?)r?["Bytes"] ?? 0
                        });
                }
                else
                {
                    // Else must be an added file.
                    fsn = new FileSystemNode(this)
                    {
                        Id = (string?)r?["Hash"],
                        Size = (string?)r?["Size"] is null ? 0UL : ulong.Parse((string?)r["Size"] ?? "0"),
                        IsDirectory = false,
                        Name = name,
                    };
                    if (this.logger.IsEnabled(LogLevel.Debug))
                    {
                        this.logger.LogDebug("added {fsnId} {fsnName}", fsn.Id, fsn.Name);
                    }
                }
            }
        }

        if (fsn is null)
        {
            throw new InvalidOperationException("No file added.");
        }

        fsn.IsDirectory = options.Wrap == true;
        return fsn;
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<IFileSystemNode> AddAsync(
        FilePart[] fileParts,
        FolderPart[] folderParts,
        AddFileOptions? options = default,
        [EnumeratorCancellation] CancellationToken cancel = default)
    {
        // Add each file part individually through the standard add API
        foreach (var filePart in fileParts)
        {
            if (filePart.Data is not null)
            {
                var node = await this.AddAsync(filePart.Data, filePart.Name, options, cancel);
                yield return node;
            }
        }
    }

    /// <inheritdoc />
    public async Task<IFileSystemNode> AddFileAsync(string path, AddFileOptions? options = null, CancellationToken cancel = default)
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        var node = await this.AddAsync(stream, Path.GetFileName(path), options, cancel);
        return node;
    }

    /// <inheritdoc />
    public Task<IFileSystemNode> AddTextAsync(string text, AddFileOptions? options = null, CancellationToken cancel = default) =>
        this.AddAsync(new MemoryStream(Encoding.UTF8.GetBytes(text), false), "", options, cancel);

    /// <inheritdoc />
    public async Task<Stream> GetAsync(string path, bool compress = false, CancellationToken cancel = default)
    {
        var stream = await this.ipfs.ExecuteCommand<Stream>("get", path, cancel, $"compress={compress}");
        return stream ?? Stream.Null;
    }

    /// <inheritdoc />
    public async Task<IFileSystemNode> ListAsync(string path, CancellationToken cancel = default)
    {
        var json = await this.ipfs.ExecuteCommand<string?>("file/ls", path, cancel);
        if (json is null)
        {
            throw new HttpRequestException("No response from file/ls");
        }

        var r = JObject.Parse(json);
        var hash = (string?)r?["Arguments"]?[path];
        var o = hash is null ? null : (JObject?)r?["Objects"]?[hash];
        var node = new FileSystemNode(this)
        {
            Id = (string?)o?["Hash"],
            Size = (ulong)(o?["Size"] ?? 0),
            IsDirectory = (string?)o?["Type"] == "Directory",
            Links = Array.Empty<FileSystemLink>(),
        };
        var links = o?["Links"] as JArray;
        if (links is not null)
        {
            node.Links = links
                .Select(l => new FileSystemLink()
                {
                    Name = (string?)l?["Name"] ?? string.Empty,
                    Id = (string?)l?["Hash"],
                    Size = (ulong)(l?["Size"] ?? 0),
                })
                .ToArray();
        }

        return node;
    }

    /// <inheritdoc />
    public async Task<string> ReadAllTextAsync(string path, CancellationToken cancel = default)
    {
        using var data = await this.ReadFileAsync(path, cancel);
        using var text = new StreamReader(data);
        return await text.ReadToEndAsync();
    }

    /// <summary>
    /// Reads the content of an existing IPFS file as text.
    /// </summary>
    /// <param name="path">A path to an existing file, such as "QmXarR6rgkQ2fDSHjSY5nM2kuCXKYGViky5nohtwgF65Ec/about"
    /// or "QmZTR5bcpQD7cFgTorqxZDYaew1Wqgfbd2ud9QqGPAkK2V"</param>
    /// <param name="host">Set a host to override the base ApiUrl</param>
    /// <param name="cancel">Is used to stop the task.  When cancelled, the <see cref="TaskCanceledException" /> is raised.</param>
    /// <returns>The contents of the <paramref name="path" /> as a <see cref="string" />.</returns>
    public async Task<string?> ReadAllTextHostAsync(string path, string host, CancellationToken cancel = default)
    {
        using var data = await this.ReadFileHostAsync(path, host, cancel);
        if (data is null)
        {
            return null;
        }

        using var text = new StreamReader(data);
        return await text.ReadToEndAsync();
    }

    /// <inheritdoc />
    public async Task<Stream> ReadFileAsync(string path, CancellationToken cancel = default)
    {
        var stream = await this.ipfs.ExecuteCommand<Stream>("cat", path, cancel);
        return stream ?? Stream.Null;
    }

    /// <inheritdoc />
    public async Task<Stream> ReadFileAsync(string path, long offset, long count = 0, CancellationToken cancel = default)
    {
        // https://github.com/ipfs/go-ipfs/issues/5380
        if (offset > int.MaxValue)
        {
            throw new NotSupportedException("Only int offsets are currently supported.");
        }

        if (count > int.MaxValue)
        {
            throw new NotSupportedException("Only int lengths are currently supported.");
        }

        if (count == 0)
        {
            count = int.MaxValue; // go-ipfs only accepts int lengths
        }

        var stream = await this.ipfs.ExecuteCommand<Stream>("cat", path, cancel, $"offset={offset}", $"length={count}");
        return stream ?? Stream.Null;
    }

    /// <summary>
    /// Opens an existing IPFS file for reading.
    /// </summary>
    /// <param name="path">A path to an existing file, such as "QmXarR6rgkQ2fDSHjSY5nM2kuCXKYGViky5nohtwgF65Ec/about"
    /// or "QmZTR5bcpQD7cFgTorqxZDYaew1Wqgfbd2ud9QqGPAkK2V"</param>
    /// <param name="host">Set a host to override the base ApiUrl</param>
    /// <param name="cancel">Is used to stop the task.  When cancelled, the <see cref="TaskCanceledException" /> is raised.</param>
    /// <returns>A <see cref="Stream" /> to the file contents.</returns>
    public Task<Stream?> ReadFileHostAsync(string path, string host, CancellationToken cancel = default) =>
        this.ipfs.ExecuteCommand<Stream>("cat", host, cancel, path);
}
