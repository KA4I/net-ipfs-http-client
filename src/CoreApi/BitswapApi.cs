// <copyright file="BitswapApi.cs" company="improvGroup, LLC">
//     Copyright © 2015-2022 Richard Schneider, Marshall Rosenstein, William Forney
// </copyright>
namespace Ipfs.Http.Client.CoreApi;

using global::Ipfs.CoreApi;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Class BitswapApi.
/// Implements the <see cref="Ipfs.CoreApi.IBitswapApi" />
/// </summary>
/// <seealso cref="Ipfs.CoreApi.IBitswapApi" />
public class BitswapApi : IBitswapApi
{
    private readonly IIpfsClient ipfs;

    /// <summary>
    /// Initializes a new instance of the <see cref="BitswapApi"/> class.
    /// </summary>
    /// <param name="ipfs">The ipfs.</param>
    /// <exception cref="System.ArgumentNullException">ipfs</exception>
    public BitswapApi(IIpfsClient ipfs)
    {
        this.ipfs = ipfs ?? throw new ArgumentNullException(nameof(ipfs));
    }

    /// <inheritdoc />
    public async Task<BitswapLedger> LedgerAsync(Peer peer, CancellationToken cancel = default)
    {
        var json = await this.ipfs.ExecuteCommand<string?>("bitswap/ledger", peer.Id.ToString(), cancel);
        if (json is null)
        {
            return new BitswapLedger { Peer = peer };
        }

        var o = JObject.Parse(json);
        return new BitswapLedger
        {
            Peer = (string?)o["Peer"],
            DataReceived = (ulong?)o["Sent"] ?? 0,
            DataSent = (ulong?)o["Recv"] ?? 0,
            BlocksExchanged = (ulong?)o["Exchanged"] ?? 0
        };
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Cid>> WantsAsync(MultiHash? peer = null, CancellationToken cancel = default)
    {
        var json = await this.ipfs.ExecuteCommand<string?>("bitswap/wantlist", peer?.ToString(), cancel);
        if (json is null)
        {
            return Enumerable.Empty<Cid>();
        }

        var keys = (JArray?)JObject.Parse(json)["Keys"];
        if (keys is null)
        {
            return Enumerable.Empty<Cid>();
        }

        // https://github.com/ipfs/go-ipfs/issues/5077
        return keys
            .Select(k =>
            {
                if (k.Type == JTokenType.String)
                {
                    return Cid.Decode(k.ToString());
                }

                var obj = (JObject)k;
                return Cid.Decode(obj["/"]?.ToString());
            });
    }
}
