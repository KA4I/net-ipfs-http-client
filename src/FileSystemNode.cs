namespace Ipfs.Http.Client;

using DotNext.Threading;
using Ipfs.CoreApi;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.Serialization;

/// <inheritdoc />
[DataContract]
public class FileSystemNode : IFileSystemNode
{
    private readonly AsyncLazy<Stream> dataStream;
    private readonly IFileSystemApi fileSystemApi;
    private bool? isDirectory;
    private IEnumerable<IFileSystemLink>? links;
    private ulong? size;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileSystemNode"/> class.
    /// </summary>
    /// <param name="fileSystemApi">The file system API.</param>
    public FileSystemNode(IFileSystemApi fileSystemApi)
    {
        this.fileSystemApi = fileSystemApi ?? IpfsContext.GetServiceProvider().GetRequiredService<IFileSystemApi>();
        this.dataStream = new AsyncLazy<Stream>(async (cancel) => await this.fileSystemApi.ReadFileAsync(this.Id, cancel));
    }

    /// <inheritdoc />
    [DataMember]
    public Cid Id { get; set; } = new Cid();

    /// <summary>
    /// Determines if the link is a directory (folder).
    /// </summary>
    /// <value>
    /// <b>true</b> if the link is a directory; Otherwise <b>false</b>, the link is some type of a file.
    /// </value>
    [DataMember]
    public bool IsDirectory
    {
        get
        {
            if (!this.isDirectory.HasValue)
            {
                this.GetInfo().Wait();
            }

            return this.isDirectory.GetValueOrDefault();
        }

        set => this.isDirectory = value;
    }

    /// <inheritdoc />
    [DataMember]
    public IEnumerable<IFileSystemLink> Links
    {
        get
        {
            if (this.links is null)
            {
                this.GetInfo().Wait();
            }

            return this.links ?? Enumerable.Empty<IFileSystemLink>();
        }

        set => this.links = value;
    }

    /// <inheritdoc />
    [DataMember]
    public string Name { get; set; } = string.Empty;

    /// <inheritdoc />
    [DataMember]
    public ulong Size
    {
        get
        {
            if (!this.size.HasValue)
            {
                this.GetInfo().Wait();
            }

            return this.size.GetValueOrDefault();
        }

        set => this.size = value;
    }

    /// <inheritdoc />
    public IFileSystemLink ToLink(string name = "") =>
        new FileSystemLink
        {
            Name = string.IsNullOrWhiteSpace(name) ? this.Name : name,
            Id = this.Id,
            Size = this.Size,
        };

    /// <summary>
    /// Gets the information.
    /// </summary>
    private async Task GetInfo()
    {
        var node = await this.fileSystemApi.ListAsync(this.Id);

        this.IsDirectory = node.IsDirectory;
        this.Links = node.Links;
        this.Size = node.Size;
    }
}
