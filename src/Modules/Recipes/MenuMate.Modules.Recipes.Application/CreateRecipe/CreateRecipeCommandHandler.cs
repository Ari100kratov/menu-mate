using MenuMate.Common.Application;
using MenuMate.Contracts.Recipes;
using MenuMate.Modules.Recipes.Application.Abstractions;
using MenuMate.Modules.Recipes.Domain.Models;
using MenuMate.SharedKernel;

namespace MenuMate.Modules.Recipes.Application.CreateRecipe;

internal sealed class CreateRecipeCommandHandler(
    IRecipesRepository repository,
    IRecipesUnitOfWork unitOfWork,
    IUserContext userContext,
    TimeProvider timeProvider)
    : ICommandHandler<CreateRecipeCommand, RecipeResponse>
{
    public async Task<Result<RecipeResponse>> Handle(
        CreateRecipeCommand command,
        CancellationToken cancellationToken)
    {
        Result<RecipeDraft> draft = RecipeRequestMapper.Map(command.Request);
        if (draft.IsFailure)
        {
            return Result.Failure<RecipeResponse>(draft.Error);
        }

        DateTimeOffset now = timeProvider.GetUtcNow();
        var recipe = Recipe.Create(Guid.CreateVersion7(), userContext.UserId, draft.Value.Title, draft.Value.Servings, now);
        recipe.UpdateDetails(draft.Value.Title, draft.Value.Servings, draft.Value.Description, draft.Value.SourceUrl, now);
        recipe.ReplaceIngredients(draft.Value.Ingredients, now);
        recipe.ReplaceSteps(draft.Value.Steps, now);
        recipe.ReplaceTags(draft.Value.Tags, now);

        await repository.AddAsync(recipe, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return RecipeMapping.ToResponse(recipe);
    }
}
