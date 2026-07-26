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
    : IQueryHandler<GetRecipesQuery, RecipeListPageResponse>
{
    public async Task<Result<RecipeListPageResponse>> Handle(
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
            return Result.Success(new RecipeListPageResponse([], 0));
        }

        RecipeCategory? category = hasValidCategory ? parsedCategory : null;
        int page = Math.Clamp(query.Page, 1, 100_000);
        int pageSize = Math.Clamp(query.PageSize, 1, 50);

        RecipeListPageReadModel readPage = await dbContext.GetRecipesAsync(
            userContext.UserId,
            catalog,
            query.Search,
            query.TagIds,
            category,
            query.FavoritesOnly,
            query.AvailableOnly,
            query.Sort,
            query.Ownership,
            (page - 1) * pageSize,
            pageSize,
            cancellationToken);
        IReadOnlyDictionary<Guid, string> tagNames = await tagCatalog.GetNamesAsync(
            readPage.Items.SelectMany(recipe => recipe.TagIds).Distinct().ToArray(),
            cancellationToken);
        IReadOnlyCollection<RecipeListItemResponse> recipes = readPage.Items
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

            return Result.Success(new RecipeListPageResponse(recipesWithCovers, readPage.TotalCount));
        }
        catch (ObjectStorageException exception)
        {
            return Result.Failure<RecipeListPageResponse>(
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
