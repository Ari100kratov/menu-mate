using System.Collections.Concurrent;
using MenuMate.Common.Application.Storage;
using Microsoft.Extensions.Caching.Memory;

namespace MenuMate.Common.Infrastructure.Storage;

/// <summary>
/// Переиспользует подписанные ссылки чтения до приближения срока их истечения.
/// </summary>
internal sealed class CachedObjectStorageService(
    IObjectStorageService innerService,
    IMemoryCache memoryCache) : IObjectStorageService
{
    private static readonly TimeSpan DefaultReadUrlLifetime = TimeSpan.FromHours(1);
    private static readonly TimeSpan MaximumRefreshAdvance = TimeSpan.FromMinutes(5);
    private readonly ConcurrentDictionary<ReadUrlCacheKey, SemaphoreSlim> _generationLocks = new();

    public Task EnsureBucketExistsAsync(string bucket, CancellationToken cancellationToken) =>
        innerService.EnsureBucketExistsAsync(bucket, cancellationToken);

    public Task PutObjectAsync(
        string bucket,
        string key,
        Stream content,
        long sizeBytes,
        string contentType,
        CancellationToken cancellationToken) =>
        innerService.PutObjectAsync(bucket, key, content, sizeBytes, contentType, cancellationToken);

    public Task DeleteObjectAsync(string bucket, string key, CancellationToken cancellationToken) =>
        innerService.DeleteObjectAsync(bucket, key, cancellationToken);

    public Task<Stream> GetObjectStreamAsync(
        string bucket,
        string key,
        CancellationToken cancellationToken) =>
        innerService.GetObjectStreamAsync(bucket, key, cancellationToken);

    public async Task<string> GetReadUrlAsync(string bucket, string key, TimeSpan? lifetime = null)
    {
        TimeSpan effectiveLifetime = lifetime ?? DefaultReadUrlLifetime;
        var cacheKey = new ReadUrlCacheKey(bucket, key, effectiveLifetime);

        if (memoryCache.TryGetValue(cacheKey, out string? cachedUrl) && cachedUrl is not null)
        {
            return cachedUrl;
        }

        SemaphoreSlim generationLock = _generationLocks.GetOrAdd(cacheKey, static _ => new SemaphoreSlim(1, 1));
        await generationLock.WaitAsync().ConfigureAwait(false);

        try
        {
            if (memoryCache.TryGetValue(cacheKey, out cachedUrl) && cachedUrl is not null)
            {
                return cachedUrl;
            }

            string readUrl = await innerService
                .GetReadUrlAsync(bucket, key, effectiveLifetime)
                .ConfigureAwait(false);

            memoryCache.Set(cacheKey, readUrl, GetCacheLifetime(effectiveLifetime));
            return readUrl;
        }
        finally
        {
            generationLock.Release();
            _generationLocks.TryRemove(new KeyValuePair<ReadUrlCacheKey, SemaphoreSlim>(cacheKey, generationLock));
        }
    }

    public Task<bool> ExistsAsync(string bucket, string key, CancellationToken cancellationToken) =>
        innerService.ExistsAsync(bucket, key, cancellationToken);

    private static TimeSpan GetCacheLifetime(TimeSpan readUrlLifetime)
    {
        if (readUrlLifetime <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(readUrlLifetime),
                readUrlLifetime,
                "Срок действия подписанной ссылки должен быть больше нуля.");
        }

        TimeSpan proportionalRefreshAdvance = readUrlLifetime / 10;
        TimeSpan refreshAdvance = proportionalRefreshAdvance < MaximumRefreshAdvance
            ? proportionalRefreshAdvance
            : MaximumRefreshAdvance;

        return readUrlLifetime - refreshAdvance;
    }

    private readonly record struct ReadUrlCacheKey(string Bucket, string Key, TimeSpan Lifetime);
}
