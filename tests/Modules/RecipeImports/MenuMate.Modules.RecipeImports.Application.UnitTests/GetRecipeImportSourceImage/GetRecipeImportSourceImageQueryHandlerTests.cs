using MenuMate.Common.Application;
using MenuMate.Common.Application.Storage;
using MenuMate.Modules.RecipeImports.Application.Abstractions;
using MenuMate.Modules.RecipeImports.Application.GetRecipeImportSourceImage;
using MenuMate.Modules.RecipeImports.Domain.Models;
using MenuMate.SharedKernel;
using MenuMate.SharedKernel.Identifiers;

namespace MenuMate.Modules.RecipeImports.Application.UnitTests.GetRecipeImportSourceImage;

public sealed class GetRecipeImportSourceImageQueryHandlerTests
{
    [Fact]
    public async Task HandleShouldReturnRequestedSourceImageForOwner()
    {
        var ownerUserId = UserId.Create();
        RecipeImportDraft draft = CreateDraft(ownerUserId);
        var storage = new StubObjectStorageService();
        var handler = new GetRecipeImportSourceImageQueryHandler(
            new StubRepository(draft),
            storage,
            new StubUserContext(ownerUserId));

        Result<RecipeImportSourceImageContent> result = await handler.Handle(
            new GetRecipeImportSourceImageQuery(draft.Id.Value, SourceImageIndex: 1),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("image/jpeg", result.Value.ContentType);
        Assert.Equal("second.jpg", result.Value.FileName);
        Assert.Equal(1, storage.ReadCount);
        await result.Value.Content.DisposeAsync();
    }

    [Fact]
    public async Task HandleShouldRejectAnotherUserWithoutReadingStorage()
    {
        RecipeImportDraft draft = CreateDraft(UserId.Create());
        var storage = new StubObjectStorageService();
        var handler = new GetRecipeImportSourceImageQueryHandler(
            new StubRepository(draft),
            storage,
            new StubUserContext(UserId.Create()));

        Result<RecipeImportSourceImageContent> result = await handler.Handle(
            new GetRecipeImportSourceImageQuery(draft.Id.Value, SourceImageIndex: 0),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(ImportApplicationErrors.AccessDenied, result.Error);
        Assert.Equal(0, storage.ReadCount);
    }

    [Fact]
    public async Task HandleShouldReturnNotFoundForMissingSourceIndex()
    {
        var ownerUserId = UserId.Create();
        RecipeImportDraft draft = CreateDraft(ownerUserId);
        var storage = new StubObjectStorageService();
        var handler = new GetRecipeImportSourceImageQueryHandler(
            new StubRepository(draft),
            storage,
            new StubUserContext(ownerUserId));

        Result<RecipeImportSourceImageContent> result = await handler.Handle(
            new GetRecipeImportSourceImageQuery(draft.Id.Value, SourceImageIndex: 2),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Imports.SourceImageNotFound", result.Error.Code);
        Assert.Equal(0, storage.ReadCount);
    }

    private static RecipeImportDraft CreateDraft(UserId ownerUserId) =>
        RecipeImportDraft.Create(
            ImportDraftId.Create(),
            ownerUserId,
            RecipeId.Create(),
            "Рецепт",
            "{}",
            "{}",
            "imports",
            "first.png",
            "image/png",
            3,
            "first.png",
            [new RecipeImportSourceImage("imports", "second.jpg", "image/jpeg", 3, "second.jpg")],
            new DateTimeOffset(2026, 7, 15, 12, 0, 0, TimeSpan.Zero));

    private sealed class StubUserContext(UserId userId) : IUserContext
    {
        public UserId UserId { get; } = userId;
    }

    private sealed class StubRepository(RecipeImportDraft? draft) : IRecipeImportDraftRepository
    {
        public Task<RecipeImportDraft?> GetByIdAsync(
            ImportDraftId id,
            CancellationToken cancellationToken) => Task.FromResult(draft);

        public Task<IReadOnlyCollection<RecipeImportDraft>> GetRecentAsync(
            UserId ownerUserId,
            int limit,
            CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<IReadOnlyCollection<RecipeImportDraft>> GetExpiredAsync(
            DateTimeOffset cutoff,
            int limit,
            CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task AddAsync(RecipeImportDraft draft, CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task UpdateAsync(RecipeImportDraft draft, CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task DeleteAsync(RecipeImportDraft draft, CancellationToken cancellationToken) =>
            throw new NotSupportedException();
    }

    private sealed class StubObjectStorageService : IObjectStorageService
    {
        public int ReadCount { get; private set; }

        public Task<Stream> GetObjectStreamAsync(
            string bucket,
            string key,
            CancellationToken cancellationToken)
        {
            ReadCount++;
            return Task.FromResult<Stream>(new MemoryStream([1, 2, 3], writable: false));
        }

        public Task EnsureBucketExistsAsync(string bucket, CancellationToken cancellationToken) =>
            throw new NotSupportedException();

        public Task PutObjectAsync(
            string bucket,
            string key,
            Stream content,
            long sizeBytes,
            string contentType,
            CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task DeleteObjectAsync(
            string bucket,
            string key,
            CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<string> GetReadUrlAsync(
            string bucket,
            string key,
            TimeSpan? lifetime = null) => throw new NotSupportedException();

        public Task<bool> ExistsAsync(
            string bucket,
            string key,
            CancellationToken cancellationToken) => throw new NotSupportedException();
    }
}
