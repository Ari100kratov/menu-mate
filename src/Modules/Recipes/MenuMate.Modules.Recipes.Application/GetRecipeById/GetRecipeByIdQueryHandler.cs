using MenuMate.Common.Application;
using MenuMate.Common.Application.Storage;
using MenuMate.Contracts.Recipes;
using MenuMate.Modules.Recipes.Application.Abstractions;
using MenuMate.Modules.Recipes.Application.RecipeImages;
using MenuMate.SharedKernel;

namespace MenuMate.Modules.Recipes.Application.GetRecipeById;

internal sealed class GetRecipeByIdQueryHandler(
    IRecipesReadDbContext dbContext,
    IUserContext userContext,
    RecipeImageReadUrlService imageReadUrlService)
    : IQueryHandler<GetRecipeByIdQuery, RecipeResponse>
{
    public async Task<Result<RecipeResponse>> Handle(
        GetRecipeByIdQuery query,
        CancellationToken cancellationToken)
    {
        RecipeResponse? recipe = await dbContext.GetRecipeAsync(
            query.RecipeId,
            userContext.UserId,
            cancellationToken);

        if (recipe is null)
        {
            return Result.Failure<RecipeResponse>(RecipeApplicationErrors.NotFound(query.RecipeId));
        }

        try
        {
            IReadOnlyCollection<RecipeImageResponse> imagesWithReadUrls =
                await imageReadUrlService.AddReadUrlsAsync(recipe.Images);

            return recipe with { Images = imagesWithReadUrls };
        }
        catch (ObjectStorageException exception)
        {
            return Result.Failure<RecipeResponse>(RecipeApplicationErrors.ImageStorageFailed(exception.Message));
        }
    }
}
