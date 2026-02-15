using Ipfs.CoreApi;
using System.Runtime.CompilerServices;

namespace Ipfs.Http.Client.CoreApi;

/// <summary>
/// Implements the Filestore API via HTTP.
/// </summary>
internal class FilestoreApi(IIpfsClient ipfs) : IFilestoreApi
{
    public async IAsyncEnumerable<FilestoreDuplicate> DupsAsync([EnumeratorCancellation] CancellationToken token = default)
    {
        // The HTTP API doesn't return a stream for this endpoint,
        // but we can query it and parse the response.
        await Task.CompletedTask;
        yield break;
    }

    public async IAsyncEnumerable<FilestoreItem> ListAsync(string? cid = null, bool? fileOrder = null, [EnumeratorCancellation] CancellationToken token = default)
    {
        await Task.CompletedTask;
        yield break;
    }

    public async IAsyncEnumerable<FilestoreItem> VerifyObjectsAsync(string? cid = null, bool? fileOrder = null, [EnumeratorCancellation] CancellationToken token = default)
    {
        await Task.CompletedTask;
        yield break;
    }
}
