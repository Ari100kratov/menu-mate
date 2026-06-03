using MenuMate.Common.Application;
using MenuMate.Modules.Recipes.Application.Abstractions;
using MenuMate.Modules.Recipes.Domain.Models;
using MenuMate.SharedKernel;

namespace MenuMate.Modules.Recipes.Application.UpdateRecipe;

internal sealed class UpdateRecipeCommandHandler(
    IRecipesRepository repository,
    IRecipesUnitOfWork unitOfWork,
    IUserContext userContext,
    TimeProvider timeProvider)
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
        recipe.UpdateDetails(draft.Value.Title, draft.Value.Servings, draft.Value.Description, draft.Value.SourceUrl, now);
        recipe.ReplaceIngredients(draft.Value.Ingredients, now);
        recipe.ReplaceSteps(draft.Value.Steps, now);
        recipe.ReplaceTags(draft.Value.Tags, now);

        await repository.UpdateAsync(recipe, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
