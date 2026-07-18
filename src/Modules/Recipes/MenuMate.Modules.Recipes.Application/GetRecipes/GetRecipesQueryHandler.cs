using MenuMate.Common.Application;
using MenuMate.Common.Application.Storage;
using MenuMate.Common.Application.Tags;
using MenuMate.Contracts.Recipes;
using MenuMate.Modules.Recipes.Application.Abstractions;
using MenuMate.Modules.Recipes.Application.RecipeImages;
using MenuMate.Modules.Recipes.Domain.Enums;
using MenuMate.SharedKernel;

namespace MenuMate.Modules.Recipes.Application.GetRecipes;

internal sealed class GetRecipesQueryHandler(
    IRecipesReadDbContext dbContext,
    IUserContext userContext,
    RecipeImageReadUrlService imageReadUrlService,
    ITagCatalog tagCatalog)
    : IQueryHandler<GetRecipesQuery, IReadOnlyCollection<RecipeListItemResponse>>
{
    public async Task<Result<IReadOnlyCollection<RecipeListItemResponse>>> Handle(
        GetRecipesQuery query,
        CancellationToken cancellationToken)
    {
        bool catalog = string.Equals(query.Scope, "catalog", StringComparison.OrdinalIgnoreCase);
        bool hasCategory = !string.IsNullOrWhiteSpace(query.Category);
        bool hasValidCategory =
            Enum.TryParse(query.Category, ignoreCase: true, out RecipeCategory parsedCategory) &&
            Enum.IsDefined(parsedCategory);
        if (hasCategory && !hasValidCategory)
        {
            return Result.Success<IReadOnlyCollection<RecipeListItemResponse>>([]);
        }

        RecipeCategory? category = hasValidCategory ? parsedCategory : null;
        int page = Math.Clamp(query.Page, 1, 100_000);
        int pageSize = Math.Clamp(query.PageSize, 1, 50);

        IReadOnlyCollection<RecipeListItemReadModel> readModels = await dbContext.GetRecipesAsync(
            userContext.UserId,
            catalog,
            query.Search,
            query.TagIds,
            category,
            query.FavoritesOnly,
            query.AvailableOnly,
            (page - 1) * pageSize,
            pageSize,
            cancellationToken);
        IReadOnlyDictionary<Guid, string> tagNames = await tagCatalog.GetNamesAsync(
            readModels.SelectMany(recipe => recipe.TagIds).Distinct().ToArray(),
            cancellationToken);
        IReadOnlyCollection<RecipeListItemResponse> recipes = readModels
            .Select(recipe => recipe.Response with
            {
                Tags = recipe.TagIds
                    .Where(tagNames.ContainsKey)
                    .Select(tagId => tagNames[tagId])
                    .OrderBy(tagName => tagName)
                    .ToArray()
            })
            .ToArray();

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
