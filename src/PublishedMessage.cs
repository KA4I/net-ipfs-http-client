namespace Ipfs.Http.Client;

using Ipfs.Http.Client.CoreApi;
using Multiformats.Base;
using Newtonsoft.Json.Linq;
using System.Runtime.Serialization;
using System.Text;

/// <summary>
///   A published message.
/// </summary>
/// <remarks>
///   The <see cref="PubSubApi"/> is used to publish and subsribe to a message.
/// </remarks>
[DataContract]
public class PublishedMessage : IPublishedMessage
{
    /// <summary>
    ///   Creates a new instance of <see cref="PublishedMessage"/> from the
    ///   specified JSON string.
    /// </summary>
    /// <param name="json">
    ///   The JSON representation of a published message.
    /// </param>
    public PublishedMessage(string json)
    {
        var o = JObject.Parse(json);
        if (o is null)
        {
            return;
        }

        var from = (string?)o["from"];
        if (from is not null)
        {
            this.Sender = from;
        }

        var seqno = (string?)o["seqno"];
        if (seqno is not null)
        {
            this.SequenceNumber = Multibase.Decode(seqno, out MultibaseEncoding _);
        }

        var data = (string?)o["data"];
        if (data is not null)
        {
            this.DataBytes = Multibase.Decode(data, out MultibaseEncoding _);
        }

        var topics = (JArray?)o["topicIDs"];
        if (topics is not null)
        {
            this.Topics = topics.Select(t => Encoding.UTF8.GetString(Multibase.Decode((string?)t, out MultibaseEncoding _))).ToArray();
        }
    }

    /// <inheritdoc />
    [DataMember]
    public byte[] DataBytes { get; private set; } = Array.Empty<byte>();

    /// <summary>
    ///   Contents as a stream.
    /// </summary>
    public Stream DataStream => new MemoryStream(this.DataBytes, false);

    /// <summary>
    ///   Contents as a string.
    /// </summary>
    /// <value>
    ///   The contents interpreted as a UTF-8 string.
    /// </value>
    public string DataString => Encoding.UTF8.GetString(this.DataBytes);

    /// <summary>>
    ///   NOT SUPPORTED.
    /// </summary>
    /// <exception cref="NotSupportedException">
    ///   A published message does not have a content id.
    /// </exception>
    public Cid Id => throw new NotSupportedException();

    /// <inheritdoc />
    [DataMember]
    public Peer Sender { get; private set; } = new Peer();

    /// <inheritdoc />
    [DataMember]
    public byte[] SequenceNumber { get; private set; } = Array.Empty<byte>();

    /// <summary>
    ///   The size of the data.
    /// </summary>
    [DataMember]
    public long Size => this.DataBytes.Length;

    /// <inheritdoc />
    [DataMember]
    public IEnumerable<string> Topics { get; private set; } = Enumerable.Empty<string>();
}
