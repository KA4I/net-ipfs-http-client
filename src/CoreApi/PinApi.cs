// <copyright file="PinApi.cs" company="improvGroup, LLC">
//     Copyright © 2015-2022 Richard Schneider, Marshall Rosenstein, William Forney
// </copyright>
namespace Ipfs.Http.Client.CoreApi;

using Ipfs.CoreApi;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Class PinApi.
/// Implements the <see cref="Ipfs.CoreApi.IPinApi" />
/// </summary>
/// <seealso cref="Ipfs.CoreApi.IPinApi" />
public class PinApi : IPinApi
{
    private readonly IIpfsClient ipfs;

    /// <summary>
    /// Initializes a new instance of the <see cref="PinApi"/> class.
    /// </summary>
    /// <param name="ipfs">The ipfs.</param>
    public PinApi(IIpfsClient ipfs) => this.ipfs = ipfs;

    /// <inheritdoc />
    public async Task<IEnumerable<Cid>> AddAsync(string path, PinAddOptions options, CancellationToken cancel = default)
    {
        var opts = new List<string>
        {
            $"recursive={options.Recursive.ToString().ToLowerInvariant()}"
        };

        if (!string.IsNullOrWhiteSpace(options.Name))
        {
            opts.Add($"name={options.Name}");
        }

        var json = await this.ipfs.ExecuteCommand<string?>("pin/add", path, cancel, opts.ToArray());
        if (json is null)
        {
            return Enumerable.Empty<Cid>();
        }

        return ((JArray?)JObject.Parse(json)?["Pins"])
            ?.Select(p => (Cid)(string)p!)
            ?? Enumerable.Empty<Cid>();
    }

    /// <inheritdoc />
    public Task<IEnumerable<Cid>> AddAsync(string path, PinAddOptions options, IProgress<BlocksPinnedProgress> progress, CancellationToken cancel = default)
    {
        // Progress is not supported via the HTTP API in the same way; delegate to the non-progress overload.
        return this.AddAsync(path, options, cancel);
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<PinListItem> ListAsync([EnumeratorCancellation] CancellationToken cancel = default)
    {
        await foreach (var item in this.ListAsync(new PinListOptions { Type = PinType.All }, cancel))
        {
            yield return item;
        }
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<PinListItem> ListAsync(PinType type, [EnumeratorCancellation] CancellationToken cancel = default)
    {
        await foreach (var item in this.ListAsync(new PinListOptions { Type = type }, cancel))
        {
            yield return item;
        }
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<PinListItem> ListAsync(PinListOptions options, [EnumeratorCancellation] CancellationToken cancel = default)
    {
        var opts = new List<string>
        {
            $"type={PinTypeToString(options.Type)}"
        };

        if (options.Quiet)
        {
            opts.Add("quiet=true");
        }

        if (options.Stream)
        {
            opts.Add("stream=true");
        }

        if (options.Names)
        {
            opts.Add("names=true");
        }

        if (!string.IsNullOrWhiteSpace(options.Name))
        {
            opts.Add($"name={options.Name}");
        }

        var json = await this.ipfs.ExecuteCommand<string?>("pin/ls", null, cancel, opts.ToArray());
        if (json is null)
        {
            yield break;
        }

        var keys = (JObject?)JObject.Parse(json)?["Keys"];
        if (keys is null)
        {
            yield break;
        }

        foreach (var p in keys.Properties())
        {
            var typeStr = (string?)p.Value?["Type"];
            yield return new PinListItem
            {
                Cid = (Cid)p.Name,
                Type = ParsePinType(typeStr),
            };
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Cid>> RemoveAsync(Cid id, bool recursive = true, CancellationToken cancel = default)
    {
        var opts = $"recursive={recursive.ToString().ToLowerInvariant()}";
        var json = await this.ipfs.ExecuteCommand<string?>("pin/rm", id, cancel, opts);
        if (json is null)
        {
            return Enumerable.Empty<Cid>();
        }

        return ((JArray?)JObject.Parse(json)?["Pins"])
            ?.Select(p => (Cid)(string)p!)
            ?? Enumerable.Empty<Cid>();
    }

    private static string PinTypeToString(PinType type) => type switch
    {
        PinType.Direct => "direct",
        PinType.Indirect => "indirect",
        PinType.Recursive => "recursive",
        _ => "all"
    };

    private static PinType ParsePinType(string? type) => type?.ToLowerInvariant() switch
    {
        "direct" => PinType.Direct,
        "indirect" => PinType.Indirect,
        "recursive" => PinType.Recursive,
        _ => PinType.All
    };
}
