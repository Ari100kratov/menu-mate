using MenuMate.Modules.Recipes.Application.Abstractions;
using MenuMate.Modules.Recipes.Application.RecipeImages;
using MenuMate.Modules.Recipes.Application.UploadRecipeImage;
using MenuMate.Modules.Recipes.Domain.Enums;
using MenuMate.Modules.Recipes.Infrastructure.Database.Entities;
using MenuMate.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace MenuMate.Modules.Recipes.Infrastructure.Database;

internal sealed class EfRecipeImagesRepository(RecipesDbContext dbContext) : IRecipeImagesRepository
{
    public async Task AddAsync(RecipeImageMetadata image, CancellationToken cancellationToken)
    {
        await dbContext.RecipeImages.AddAsync(RecipeImageRecord.FromMetadata(image), cancellationToken);
    }

    public async Task<IReadOnlyCollection<RecipeImageObjectReference>> MarkActiveImagesDeletedAsync(
        Guid recipeId,
        UserId ownerUserId,
        RecipeImageScope scope,
        int? stepNumber,
        CancellationToken cancellationToken)
    {
        List<RecipeImageRecord> images = await dbContext.RecipeImages
            .Where(image =>
                image.RecipeId == recipeId &&
                image.OwnerUserId == ownerUserId &&
                image.Scope == scope &&
                image.StepNumber == stepNumber &&
                !image.IsDeleted)
            .ToListAsync(cancellationToken);

        foreach (RecipeImageRecord image in images)
        {
            image.IsDeleted = true;
        }

        return images.Select(ToObjectReference).ToArray();
    }

    public async Task<RecipeImageObjectReference?> MarkActiveImageDeletedAsync(
        Guid recipeId,
        Guid imageId,
        UserId ownerUserId,
        CancellationToken cancellationToken)
    {
        RecipeImageRecord? image = await dbContext.RecipeImages
            .SingleOrDefaultAsync(
                image =>
                    image.Id == imageId &&
                    image.RecipeId == recipeId &&
                    image.OwnerUserId == ownerUserId &&
                    !image.IsDeleted,
                cancellationToken);

        if (image is null)
        {
            return null;
        }

        image.IsDeleted = true;
        return ToObjectReference(image);
    }

    private static RecipeImageObjectReference ToObjectReference(RecipeImageRecord image) =>
        new(
            image.Id,
            image.OwnerUserId,
            image.RecipeId,
            image.Scope,
            image.BucketName,
            image.ObjectKey);
}
