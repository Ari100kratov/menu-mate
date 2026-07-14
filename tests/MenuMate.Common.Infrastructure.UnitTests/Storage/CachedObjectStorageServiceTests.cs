using MenuMate.Common.Application.Storage;
using MenuMate.Common.Infrastructure.Storage;
using Microsoft.Extensions.Caching.Memory;

namespace MenuMate.Common.Infrastructure.UnitTests.Storage;

public sealed class CachedObjectStorageServiceTests
{
    [Fact]
    public async Task GetReadUrlAsyncShouldReuseUrlForSameObjectAndLifetime()
    {
        var innerService = new StubObjectStorageService();
        using var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var service = new CachedObjectStorageService(innerService, memoryCache);

        string firstUrl = await service.GetReadUrlAsync("images", "recipes/cover.jpg", TimeSpan.FromHours(1));
        string secondUrl = await service.GetReadUrlAsync("images", "recipes/cover.jpg", TimeSpan.FromHours(1));

        Assert.Equal(firstUrl, secondUrl);
        Assert.Equal(1, innerService.ReadUrlRequestCount);
    }

    [Fact]
    public async Task GetReadUrlAsyncShouldCoalesceConcurrentRequests()
    {
        var innerService = new StubObjectStorageService();
        using var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var service = new CachedObjectStorageService(innerService, memoryCache);

        Task<string>[] requests =
        [
            .. Enumerable.Range(0, 20)
                .Select(_ => service.GetReadUrlAsync("images", "recipes/cover.jpg", TimeSpan.FromHours(1)))
        ];

        string[] urls = await Task.WhenAll(requests);

        Assert.Single(urls.Distinct(StringComparer.Ordinal));
        Assert.Equal(1, innerService.ReadUrlRequestCount);
    }

    [Fact]
    public async Task GetReadUrlAsyncShouldKeepDifferentLifetimesInSeparateCacheEntries()
    {
        var innerService = new StubObjectStorageService();
        using var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var service = new CachedObjectStorageService(innerService, memoryCache);

        string shortUrl = await service.GetReadUrlAsync("images", "recipes/cover.jpg", TimeSpan.FromMinutes(30));
        string longUrl = await service.GetReadUrlAsync("images", "recipes/cover.jpg", TimeSpan.FromHours(1));

        Assert.NotEqual(shortUrl, longUrl);
        Assert.Equal(2, innerService.ReadUrlRequestCount);
    }

    private sealed class StubObjectStorageService : IObjectStorageService
    {
        private int _readUrlRequestCount;

        public int ReadUrlRequestCount => _readUrlRequestCount;

        public Task<string> GetReadUrlAsync(string bucket, string key, TimeSpan? lifetime = null)
        {
            int requestNumber = Interlocked.Increment(ref _readUrlRequestCount);
            return Task.FromResult($"https://storage.example/{bucket}/{key}?request={requestNumber}");
        }

        public Task EnsureBucketExistsAsync(string bucket, CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task PutObjectAsync(
            string bucket,
            string key,
            Stream content,
            long sizeBytes,
            string contentType,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task DeleteObjectAsync(string bucket, string key, CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<Stream> GetObjectStreamAsync(
            string bucket,
            string key,
            CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task<bool> ExistsAsync(string bucket, string key, CancellationToken cancellationToken) =>
            throw new NotSupportedException();
    }
}
