using System.Collections.Concurrent;
using MenuMate.Common.Application.Storage;

namespace MenuMate.Api.IntegrationTests;

internal sealed class InMemoryObjectStorageService : IObjectStorageService
{
    private readonly ConcurrentDictionary<(string Bucket, string Key), byte[]> _objects = new();

    public Task EnsureBucketExistsAsync(string bucket, CancellationToken cancellationToken) =>
        Task.CompletedTask;

    public async Task PutObjectAsync(
        string bucket,
        string key,
        Stream content,
        long sizeBytes,
        string contentType,
        CancellationToken cancellationToken)
    {
        using var buffer = new MemoryStream();
        await content.CopyToAsync(buffer, cancellationToken);
        byte[] bytes = buffer.ToArray();
        if (bytes.LongLength != sizeBytes)
        {
            throw new InvalidOperationException("Stored object size does not match declared size.");
        }

        _objects[(bucket, key)] = bytes;
    }

    public Task DeleteObjectAsync(string bucket, string key, CancellationToken cancellationToken)
    {
        _objects.TryRemove((bucket, key), out _);
        return Task.CompletedTask;
    }

    public Task<Stream> GetObjectStreamAsync(
        string bucket,
        string key,
        CancellationToken cancellationToken)
    {
        if (!_objects.TryGetValue((bucket, key), out byte[]? bytes))
        {
            throw new ObjectStorageObjectNotFoundException(bucket, key);
        }

        return Task.FromResult<Stream>(new MemoryStream(bytes, writable: false));
    }

    public Task<string> GetReadUrlAsync(string bucket, string key, TimeSpan? lifetime = null) =>
        Task.FromResult($"https://storage.test/{Uri.EscapeDataString(bucket)}/{Uri.EscapeDataString(key)}");

    public Task<bool> ExistsAsync(string bucket, string key, CancellationToken cancellationToken) =>
        Task.FromResult(_objects.ContainsKey((bucket, key)));

    public byte[] GetBytes(string bucket, string key) => _objects[(bucket, key)];
}
