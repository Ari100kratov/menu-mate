using MenuMate.Common.Application;
using MenuMate.Common.Application.Tags;
using MenuMate.Contracts.Recipes;
using MenuMate.Modules.Recipes.Application.Abstractions;
using MenuMate.Modules.Recipes.Domain.Models;
using MenuMate.Modules.Recipes.Domain.ValueObjects;
using MenuMate.SharedKernel;

namespace MenuMate.Modules.Recipes.Application.CreateRecipe;

internal sealed class CreateRecipeCommandHandler(
    IRecipesRepository repository,
    IRecipesUnitOfWork unitOfWork,
    IUserContext userContext,
    TimeProvider timeProvider,
    RecipeProductResolver productResolver,
    RecipeTagCatalogResolver tagCatalogResolver)
    : ICommandHandler<CreateRecipeCommand, RecipeResponse>
{
    public async Task<Result<RecipeResponse>> Handle(
        CreateRecipeCommand command,
        CancellationToken cancellationToken)
    {
        if (command.RecipeId.HasValue)
        {
            Recipe? existingRecipe = await repository.GetByIdAsync(
                command.RecipeId.Value,
                cancellationToken);
            if (existingRecipe is not null)
            {
                if (existingRecipe.OwnerUserId != userContext.UserId)
                {
                    return Result.Failure<RecipeResponse>(RecipeApplicationErrors.AccessDenied);
                }

                return RecipeMapping.ToResponse(existingRecipe, userContext.UserId);
            }
        }

        Result<RecipeDraft> draft = RecipeRequestMapper.Map(command.Request);
        if (draft.IsFailure)
        {
            return Result.Failure<RecipeResponse>(draft.Error);
        }

        DateTimeOffset now = timeProvider.GetUtcNow();
        Result<IReadOnlyCollection<RecipeIngredient>> ingredients = await productResolver.ResolveAsync(
            draft.Value.Ingredients,
            cancellationToken);
        if (ingredients.IsFailure)
        {
            return Result.Failure<RecipeResponse>(ingredients.Error);
        }

        Result<IReadOnlyCollection<RecipeTag>> tags = await tagCatalogResolver.ResolveAsync(
            draft.Value.Tags,
            command.TagSource,
            cancellationToken);
        if (tags.IsFailure)
        {
            return Result.Failure<RecipeResponse>(tags.Error);
        }

        var recipe = Recipe.Create(
            command.RecipeId ?? Guid.CreateVersion7(),
            userContext.UserId,
            draft.Value.Title,
            draft.Value.Servings,
            draft.Value.Category,
            draft.Value.Visibility,
            now);
        recipe.UpdateDetails(
            draft.Value.Title,
            draft.Value.Servings,
            draft.Value.Category,
            draft.Value.Visibility,
            draft.Value.TotalTimeMinutes,
            draft.Value.ActiveTimeMinutes,
            draft.Value.Description,
            draft.Value.SourceUrl,
            now);
        recipe.ReplaceIngredients(ingredients.Value, now);
        recipe.ReplaceSteps(draft.Value.Steps, now);
        recipe.ReplaceTags(tags.Value, now);

        await repository.AddAsync(recipe, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return RecipeMapping.ToResponse(recipe, userContext.UserId);
    }
}
