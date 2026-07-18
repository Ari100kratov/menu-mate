using MenuMate.Common.Application;
using MenuMate.Common.Application.Storage;
using MenuMate.Common.Application.Tags;
using MenuMate.Contracts.Recipes;
using MenuMate.Modules.Recipes.Application.Abstractions;
using MenuMate.Modules.Recipes.Application.RecipeImages;
using MenuMate.SharedKernel;

namespace MenuMate.Modules.Recipes.Application.GetRecipeById;

internal sealed class GetRecipeByIdQueryHandler(
    IRecipesReadDbContext dbContext,
    IUserContext userContext,
    RecipeImageReadUrlService imageReadUrlService,
    ITagCatalog tagCatalog)
    : IQueryHandler<GetRecipeByIdQuery, RecipeResponse>
{
    public async Task<Result<RecipeResponse>> Handle(
        GetRecipeByIdQuery query,
        CancellationToken cancellationToken)
    {
        RecipeReadModel? readModel = await dbContext.GetRecipeAsync(
            query.RecipeId,
            query.RevisionId,
            userContext.UserId,
            cancellationToken);

        if (readModel is null)
        {
            return Result.Failure<RecipeResponse>(RecipeApplicationErrors.NotFound(query.RecipeId));
        }

        IReadOnlyDictionary<Guid, string> tagNames = await tagCatalog.GetNamesAsync(
            readModel.TagIds,
            cancellationToken);
        RecipeResponse recipe = readModel.Response with
        {
            Tags = readModel.TagIds
                .Where(tagNames.ContainsKey)
                .Select(tagId => tagNames[tagId])
                .OrderBy(tagName => tagName)
                .ToArray()
        };

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
