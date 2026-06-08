using MenuMate.Common.Application;
using MenuMate.Common.Application.Storage;
using MenuMate.Contracts.Recipes;
using MenuMate.Modules.Recipes.Application.Abstractions;
using MenuMate.Modules.Recipes.Application.RecipeImages;
using MenuMate.SharedKernel;

namespace MenuMate.Modules.Recipes.Application.GetRecipes;

internal sealed class GetRecipesQueryHandler(
    IRecipesReadDbContext dbContext,
    IUserContext userContext,
    RecipeImageReadUrlService imageReadUrlService)
    : IQueryHandler<GetRecipesQuery, IReadOnlyCollection<RecipeListItemResponse>>
{
    public async Task<Result<IReadOnlyCollection<RecipeListItemResponse>>> Handle(
        GetRecipesQuery query,
        CancellationToken cancellationToken)
    {
        string? normalizedTag = string.IsNullOrWhiteSpace(query.Tag)
            ? null
            : TextNormalizer.NormalizeSearchText(query.Tag);

        bool catalog = string.Equals(query.Scope, "catalog", StringComparison.OrdinalIgnoreCase);

        IReadOnlyCollection<RecipeListItemResponse> recipes = await dbContext.GetRecipesAsync(
            userContext.UserId,
            catalog,
            query.Search,
            normalizedTag,
            query.FavoritesOnly,
            cancellationToken);

        try
        {
            IReadOnlyCollection<RecipeListItemResponse> recipesWithCovers =
                await AddCoverReadUrlsAsync(recipes);

            return Result.Success(recipesWithCovers);
        }
        catch (ObjectStorageException exception)
        {
            return Result.Failure<IReadOnlyCollection<RecipeListItemResponse>>(
                RecipeApplicationErrors.ImageStorageFailed(exception.Message));
        }
    }

    private async Task<IReadOnlyCollection<RecipeListItemResponse>> AddCoverReadUrlsAsync(
        IReadOnlyCollection<RecipeListItemResponse> recipes)
    {
        var recipesWithCovers = new List<RecipeListItemResponse>(recipes.Count);
        foreach (RecipeListItemResponse recipe in recipes)
        {
            RecipeImageResponse? coverImage = recipe.CoverImage is null
                ? null
                : await imageReadUrlService.AddReadUrlAsync(recipe.CoverImage);

            recipesWithCovers.Add(recipe with { CoverImage = coverImage });
        }

        return recipesWithCovers;
    }
}
