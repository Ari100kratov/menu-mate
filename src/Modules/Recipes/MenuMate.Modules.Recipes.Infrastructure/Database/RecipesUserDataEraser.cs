using MenuMate.Common.Application;
using MenuMate.Common.Application.Storage;
using MenuMate.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace MenuMate.Modules.Recipes.Infrastructure.Database;

internal sealed class RecipesUserDataEraser(
    RecipesDbContext dbContext,
    IObjectStorageService objectStorageService) : IUserDataEraser
{
    public int Order => 400;

    public async Task EraseAsync(UserId userId, CancellationToken cancellationToken)
    {
        Guid[] recipeIds = await dbContext.Recipes
            .AsNoTracking()
            .Where(recipe => recipe.OwnerUserId == userId)
            .Select(recipe => recipe.Id)
            .ToArrayAsync(cancellationToken);

        ImageObjectReference[] images = await dbContext.RecipeImages
            .AsNoTracking()
            .Where(image => image.OwnerUserId == userId)
            .Select(image => new ImageObjectReference(image.BucketName, image.ObjectKey))
            .ToArrayAsync(cancellationToken);

        foreach (ImageObjectReference image in images)
        {
            await objectStorageService.DeleteObjectAsync(image.BucketName, image.ObjectKey, cancellationToken);
        }

        await using IDbContextTransaction transaction =
            await dbContext.Database.BeginTransactionAsync(cancellationToken);
        await dbContext.RecipeLibraryEntries
            .Where(entry => entry.UserId == userId || recipeIds.Contains(entry.RecipeId))
            .ExecuteDeleteAsync(cancellationToken);
        await dbContext.Recipes
            .Where(recipe => recipe.OwnerUserId == userId)
            .ExecuteDeleteAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    private sealed record ImageObjectReference(string BucketName, string ObjectKey);
}
