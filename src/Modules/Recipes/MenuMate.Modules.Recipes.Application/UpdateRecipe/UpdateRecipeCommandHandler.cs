using MenuMate.Common.Application;
using MenuMate.Modules.Recipes.Application.Abstractions;
using MenuMate.Modules.Recipes.Domain.Models;
using MenuMate.SharedKernel;

namespace MenuMate.Modules.Recipes.Application.UpdateRecipe;

internal sealed class UpdateRecipeCommandHandler(
    IRecipesRepository repository,
    IRecipesUnitOfWork unitOfWork,
    IUserContext userContext,
    TimeProvider timeProvider,
    RecipeProductResolver productResolver)
    : ICommandHandler<UpdateRecipeCommand>
{
    public async Task<Result> Handle(UpdateRecipeCommand command, CancellationToken cancellationToken)
    {
        Recipe? recipe = await repository.GetByIdAsync(command.RecipeId, cancellationToken);
        if (recipe is null)
        {
            return Result.Failure(RecipeApplicationErrors.NotFound(command.RecipeId));
        }

        if (recipe.OwnerUserId != userContext.UserId)
        {
            return Result.Failure(RecipeApplicationErrors.AccessDenied);
        }

        Result<RecipeDraft> draft = RecipeRequestMapper.Map(command.Request);
        if (draft.IsFailure)
        {
            return Result.Failure(draft.Error);
        }

        DateTimeOffset now = timeProvider.GetUtcNow();
        Result<IReadOnlyCollection<RecipeIngredient>> ingredients = await productResolver.ResolveAsync(
            draft.Value.Ingredients,
            cancellationToken);
        if (ingredients.IsFailure)
        {
            return Result.Failure(ingredients.Error);
        }

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
        recipe.ReplaceTags(draft.Value.Tags, now);
        recipe.PublishRevision(now);

        await repository.UpdateAsync(recipe, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
