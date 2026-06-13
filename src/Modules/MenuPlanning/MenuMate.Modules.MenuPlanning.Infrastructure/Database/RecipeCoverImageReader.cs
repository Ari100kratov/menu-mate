using MenuMate.Common.Application.Storage;
using MenuMate.Modules.MenuPlanning.Application.Abstractions;
using MenuMate.Modules.MenuPlanning.Infrastructure.Database.Source;
using MenuMate.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace MenuMate.Modules.MenuPlanning.Infrastructure.Database;

internal sealed class RecipeCoverImageReader(
    MenuPlanningDbContext dbContext,
    IObjectStorageService objectStorageService)
    : IRecipeCoverImageReader
{
    public async Task<IReadOnlyDictionary<Guid, Uri>> GetReadUrlsAsync(
        IReadOnlyCollection<RecipeId> recipeIds,
        CancellationToken cancellationToken)
    {
        if (recipeIds.Count == 0)
        {
            return new Dictionary<Guid, Uri>();
        }

        RecipeCoverImageSourceRecord[] images = await dbContext.Set<RecipeCoverImageSourceRecord>()
            .AsNoTracking()
            .Where(image =>
                recipeIds.Contains(image.RecipeId) &&
                image.Scope == "Cover" &&
                !image.IsDeleted)
            .OrderByDescending(image => image.CreatedAt)
            .ToArrayAsync(cancellationToken);

        RecipeCoverImageSourceRecord[] latestImages =
        [
            .. images
                .GroupBy(image => image.RecipeId)
                .Select(group => group.First())
        ];
        KeyValuePair<Guid, Uri>[] readUrls = await Task.WhenAll(latestImages.Select(async image =>
        {
            string readUrl = await objectStorageService.GetReadUrlAsync(image.BucketName, image.ObjectKey);
            return KeyValuePair.Create(image.RecipeId.Value, new Uri(readUrl, UriKind.Absolute));
        }));

        return readUrls.ToDictionary();
    }
}
