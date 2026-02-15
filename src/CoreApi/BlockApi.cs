// <copyright file="BlockApi.cs" company="improvGroup, LLC">
//     Copyright © 2015-2022 Richard Schneider, Marshall Rosenstein, William Forney
// </copyright>
namespace Ipfs.Http.Client.CoreApi;

using Ipfs.CoreApi;
using Newtonsoft.Json.Linq;

/// <summary>
/// Class BlockApi.
/// Implements the <see cref="Ipfs.CoreApi.IBlockApi" />
/// </summary>
/// <seealso cref="Ipfs.CoreApi.IBlockApi" />
public class BlockApi : IBlockApi
{
    private readonly IIpfsClient ipfs;

    /// <summary>
    /// Initializes a new instance of the <see cref="BlockApi"/> class.
    /// </summary>
    /// <param name="ipfs">The ipfs.</param>
    /// <exception cref="System.ArgumentNullException">ipfs</exception>
    public BlockApi(IIpfsClient ipfs)
    {
        this.ipfs = ipfs ?? throw new ArgumentNullException(nameof(ipfs));
    }

    /// <inheritdoc />
    public async Task<byte[]> GetAsync(Cid id, CancellationToken cancel = default)
    {
        var data = await this.ipfs.ExecuteCommand<byte[]>("block/get", id, cancel);
        return data ?? Array.Empty<byte>();
    }

    /// <inheritdoc />
    public async Task<IBlockStat> PutAsync(
        byte[] data,
        string cidCodec = "raw",
        MultiHash? hash = null,
        bool? pin = null,
        bool? allowBigBlock = null,
        CancellationToken cancel = default)
    {
        var options = new List<string>();
        if (cidCodec != "raw")
        {
            options.Add($"cid-codec={cidCodec}");
        }

        if (hash is not null)
        {
            options.Add($"mhtype={hash}");
        }

        if (pin.HasValue)
        {
            options.Add($"pin={pin.Value.ToString().ToLowerInvariant()}");
        }

        if (allowBigBlock.HasValue)
        {
            options.Add($"allow-big-block={allowBigBlock.Value.ToString().ToLowerInvariant()}");
        }

        var json = await this.ipfs.ExecuteCommand<byte[], string?>("block/put", null, data, IpfsClient.IpfsHttpClientName, cancel, options.ToArray());
        if (json is null)
        {
            throw new HttpRequestException("No response from block/put");
        }

        var info = JObject.Parse(json);
        return new Block
        {
            Id = (string?)info["Key"] ?? string.Empty,
            Size = (int?)info["Size"] ?? data.Length
        };
    }

    /// <inheritdoc />
    public async Task<IBlockStat> PutAsync(
        Stream data,
        string cidCodec = "raw",
        MultiHash? hash = null,
        bool? pin = null,
        bool? allowBigBlock = null,
        CancellationToken cancel = default)
    {
        var options = new List<string>();
        if (cidCodec != "raw")
        {
            options.Add($"cid-codec={cidCodec}");
        }

        if (hash is not null)
        {
            options.Add($"mhtype={hash}");
        }

        if (pin.HasValue)
        {
            options.Add($"pin={pin.Value.ToString().ToLowerInvariant()}");
        }

        if (allowBigBlock.HasValue)
        {
            options.Add($"allow-big-block={allowBigBlock.Value.ToString().ToLowerInvariant()}");
        }

        var json = await this.ipfs.ExecuteCommand<Stream?, string?>("block/put", data: data, cancellationToken: cancel, options: options.ToArray());
        if (json is null)
        {
            throw new HttpRequestException("No response from block/put");
        }

        var info = JObject.Parse(json);
        return new Block
        {
            Id = (string?)info["Key"] ?? string.Empty,
            Size = (int?)info["Size"] ?? 0
        };
    }

    /// <inheritdoc />
    public async Task<Cid> RemoveAsync(Cid id, bool ignoreNonexistent = false, CancellationToken cancel = default)
    {
        var json = await this.ipfs.ExecuteCommand<string?>("block/rm", id, cancel, "force=" + ignoreNonexistent.ToString().ToLowerInvariant());
        if (json is null || json.Length == 0)
        {
            return id;
        }

        var result = JObject.Parse(json);
        var error = (string?)result["Error"];
        return error is null ? (Cid)(string?)result["Hash"]! : throw new HttpRequestException(error);
    }

    /// <inheritdoc />
    public async Task<IBlockStat> StatAsync(Cid id, CancellationToken cancel = default)
    {
        var json = await this.ipfs.ExecuteCommand<string?>("block/stat", id, cancel);
        if (json is null)
        {
            throw new HttpRequestException("No response from block/stat");
        }

        var info = JObject.Parse(json);
        return new Block
        {
            Size = (int?)info?["Size"] ?? 0,
            Id = (string?)info?["Key"] ?? string.Empty
        };
    }
}
