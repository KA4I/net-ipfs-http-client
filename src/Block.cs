namespace Ipfs.Http.Client;

using System.IO;
using System.Runtime.Serialization;

/// <inheritdoc />
[DataContract]
[Serializable]
public class Block : IBlockStat
{
    private int? size;

    /// <summary>
    /// The raw data bytes of the block.
    /// </summary>
    [DataMember]
    public byte[] DataBytes { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// The data as a stream.
    /// </summary>
    public Stream DataStream => new MemoryStream(this.DataBytes, false);

    /// <inheritdoc />
    [DataMember]
    public Cid Id { get; set; } = new Cid();

    /// <inheritdoc />
    [DataMember]
    public int Size
    {
        get => this.size ?? this.DataBytes.Length;
        set => this.size = value;
    }
}
