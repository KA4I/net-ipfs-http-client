namespace Ipfs.Http.Client.CoreApi;

using Ipfs.CoreApi;

/// <summary>
/// Stub implementation of <see cref="IMfsApi"/>.
/// </summary>
public class MfsApi : IMfsApi
{
    /// <inheritdoc />
    public Task CopyAsync(string sourceMfsPathOrCid, string destMfsPath, bool? parents = null, CancellationToken cancel = default) =>
        throw new NotImplementedException();

    /// <inheritdoc />
    public Task<Cid> FlushAsync(string? path = null, CancellationToken cancel = default) =>
        throw new NotImplementedException();

    /// <inheritdoc />
    public Task<IEnumerable<IFileSystemNode>> ListAsync(string path, bool? U = null, CancellationToken cancel = default) =>
        throw new NotImplementedException();

    /// <inheritdoc />
    public Task MakeDirectoryAsync(string path, bool? parents = null, int? cidVersion = null, string? multiHash = null, CancellationToken cancel = default) =>
        throw new NotImplementedException();

    /// <inheritdoc />
    public Task MoveAsync(string sourceMfsPath, string destMfsPath, CancellationToken cancel = default) =>
        throw new NotImplementedException();

    /// <inheritdoc />
    public Task<string> ReadFileAsync(string path, long? offset = null, long? count = null, CancellationToken cancel = default) =>
        throw new NotImplementedException();

    /// <inheritdoc />
    public Task<Stream> ReadFileStreamAsync(string path, long? offset = null, long? count = null, CancellationToken cancel = default) =>
        throw new NotImplementedException();

    /// <inheritdoc />
    public Task RemoveAsync(string path, bool? recursive = null, bool? force = null, CancellationToken cancel = default) =>
        throw new NotImplementedException();

    /// <inheritdoc />
    public Task<FileStatResult> StatAsync(string path, CancellationToken cancel = default) =>
        throw new NotImplementedException();

    /// <inheritdoc />
    public Task<FileStatWithLocalityResult> StatAsync(string path, bool withLocal, CancellationToken cancel = default) =>
        throw new NotImplementedException();

    /// <inheritdoc />
    public Task WriteAsync(string path, string text, MfsWriteOptions options, CancellationToken cancel = default) =>
        throw new NotImplementedException();

    /// <inheritdoc />
    public Task WriteAsync(string path, byte[] data, MfsWriteOptions options, CancellationToken cancel = default) =>
        throw new NotImplementedException();

    /// <inheritdoc />
    public Task WriteAsync(string path, Stream data, MfsWriteOptions options, CancellationToken cancel = default) =>
        throw new NotImplementedException();
}
