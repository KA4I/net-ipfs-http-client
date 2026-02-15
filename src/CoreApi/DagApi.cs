// <copyright file="DagApi.cs" company="improvGroup, LLC">
//     Copyright © 2015-2022 Richard Schneider, Marshall Rosenstein, William Forney
// </copyright>
namespace Ipfs.Http.Client.CoreApi;

using Ipfs.CoreApi;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Class DagApi.
/// Implements the <see cref="Ipfs.CoreApi.IDagApi" />
/// </summary>
/// <seealso cref="Ipfs.CoreApi.IDagApi" />
public class DagApi : IDagApi
{
    private readonly IIpfsClient ipfs;

    /// <summary>
    /// Initializes a new instance of the <see cref="DagApi"/> class.
    /// </summary>
    /// <param name="ipfs">The ipfs.</param>
    public DagApi(IIpfsClient ipfs) => this.ipfs = ipfs;

    /// <inheritdoc />
    public async Task<JObject> GetAsync(Cid id, string outputCodec = "dag-json", CancellationToken cancel = default)
    {
        var json = await this.ipfs.ExecuteCommand<string?>("dag/get", id, cancel, $"output-codec={outputCodec}");
        return json is null ? new JObject() : JObject.Parse(json);
    }

    /// <inheritdoc />
    public async Task<JToken> GetAsync(string path, string outputCodec = "dag-json", CancellationToken cancel = default)
    {
        var json = await this.ipfs.ExecuteCommand<string?>("dag/get", path, cancel, $"output-codec={outputCodec}");
        return json is null ? JValue.CreateNull() : JToken.Parse(json);
    }

    /// <inheritdoc />
    public async Task<T> GetAsync<T>(Cid id, string outputCodec = "dag-json", CancellationToken cancel = default)
    {
        var json = await this.ipfs.ExecuteCommand<string?>("dag/get", id, cancel, $"output-codec={outputCodec}");
        return json is null ? default! : JsonConvert.DeserializeObject<T>(json)!;
    }

    /// <inheritdoc />
    public async Task<Cid> PutAsync(
        JObject data,
        string storeCodec = "dag-cbor",
        string inputCodec = "dag-json",
        bool? pin = null,
        MultiHash? hash = null,
        bool? allowBigBlock = null,
        CancellationToken cancel = default)
    {
        using var ms = new MemoryStream();
        using var sw = new StreamWriter(ms, new UTF8Encoding(false), 4096, true) { AutoFlush = true };
        using (var jw = new JsonTextWriter(sw))
        {
            var serializer = new JsonSerializer
            {
                Culture = CultureInfo.InvariantCulture
            };
            serializer.Serialize(jw, data);
        }

        ms.Position = 0;
        return await this.PutAsync(ms, storeCodec, inputCodec, pin, hash, allowBigBlock, cancel);
    }

    /// <inheritdoc />
    public async Task<Cid> PutAsync(
        object data,
        string storeCodec = "dag-cbor",
        string inputCodec = "dag-json",
        bool? pin = null,
        MultiHash? hash = null,
        bool? allowBigBlock = null,
        CancellationToken cancel = default)
    {
        using var ms = new MemoryStream(
            Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data)),
            false);
        return await this.PutAsync(ms, storeCodec, inputCodec, pin, hash, allowBigBlock, cancel);
    }

    /// <inheritdoc />
    public async Task<Cid> PutAsync(
        Stream data,
        string storeCodec = "dag-cbor",
        string inputCodec = "dag-json",
        bool? pin = null,
        MultiHash? hash = null,
        bool? allowBigBlock = null,
        CancellationToken cancel = default)
    {
        var opts = new List<string>
        {
            $"store-codec={storeCodec}",
            $"input-codec={inputCodec}"
        };

        if (pin.HasValue)
        {
            opts.Add($"pin={pin.Value.ToString().ToLowerInvariant()}");
        }

        if (hash is not null)
        {
            opts.Add($"hash={hash}");
        }

        if (allowBigBlock.HasValue)
        {
            opts.Add($"allow-big-block={allowBigBlock.Value.ToString().ToLowerInvariant()}");
        }

        var json = await this.ipfs.ExecuteCommand<Stream?, string?>(
            "dag/put",
            null,
            data,
            "unknown",
            cancel,
            opts.ToArray());
        if (json is null)
        {
            throw new HttpRequestException("No response from dag/put");
        }

        var result = JObject.Parse(json);
        return (Cid)(string?)result?["Cid"]?["/"]!;
    }

    /// <inheritdoc />
    public async Task<DagResolveOutput> ResolveAsync(string path, CancellationToken cancel = default)
    {
        var json = await this.ipfs.ExecuteCommand<string?>("dag/resolve", path, cancel);
        if (json is null)
        {
            throw new HttpRequestException("No response from dag/resolve");
        }

        return JsonConvert.DeserializeObject<DagResolveOutput>(json)!;
    }

    /// <inheritdoc />
    public async Task<DagStatSummary> StatAsync(string cid, IProgress<DagStatSummary>? progress = null, CancellationToken cancel = default)
    {
        var opts = new List<string>();
        if (progress is not null)
        {
            opts.Add("progress=true");
        }

        var json = await this.ipfs.ExecuteCommand<string?>("dag/stat", cid, cancel, opts.ToArray());
        if (json is null)
        {
            throw new HttpRequestException("No response from dag/stat");
        }

        var result = JsonConvert.DeserializeObject<DagStatSummary>(json)!;
        progress?.Report(result);
        return result;
    }

    /// <inheritdoc />
    public async Task<Stream> ExportAsync(string cid, CancellationToken cancellationToken = default)
    {
        var stream = await this.ipfs.ExecuteCommand<Stream>("dag/export", cid, cancellationToken);
        return stream ?? Stream.Null;
    }

    /// <inheritdoc />
    public async Task<CarImportOutput> ImportAsync(Stream stream, bool? pinRoots = null, bool stats = false, CancellationToken cancellationToken = default)
    {
        var opts = new List<string>();
        if (pinRoots.HasValue)
        {
            opts.Add($"pin-roots={pinRoots.Value.ToString().ToLowerInvariant()}");
        }

        if (stats)
        {
            opts.Add("stats=true");
        }

        var json = await this.ipfs.ExecuteCommand<Stream?, string?>("dag/import", data: stream, cancellationToken: cancellationToken, options: opts.ToArray());
        if (json is null)
        {
            throw new HttpRequestException("No response from dag/import");
        }

        return JsonConvert.DeserializeObject<CarImportOutput>(json)!;
    }
}
