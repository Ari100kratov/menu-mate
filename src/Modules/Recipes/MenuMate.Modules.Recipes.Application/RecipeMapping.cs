using MenuMate.Contracts.Recipes;
using MenuMate.Modules.Recipes.Domain.Models;
using MenuMate.SharedKernel.Identifiers;

namespace MenuMate.Modules.Recipes.Application;

internal static class RecipeMapping
{
    public static RecipeResponse ToResponse(
        Recipe recipe,
        UserId currentUserId,
        bool isFavorite = false,
        Guid? savedRevisionId = null) =>
        new(
            recipe.Id,
            recipe.CurrentRevisionId.Value,
            recipe.CurrentRevisionId.Value,
            savedRevisionId,
            recipe.RevisionNumber,
            recipe.OwnerUserId == currentUserId,
            isFavorite,
            savedRevisionId == recipe.CurrentRevisionId.Value,
            "Current",
            recipe.SourceRecipeId,
            recipe.SourceRevisionId?.Value,
            recipe.Title.Value,
            recipe.Description,
            recipe.Servings.Value,
            recipe.Category.ToString(),
            recipe.Visibility.ToString(),
            recipe.TotalTimeMinutes,
            recipe.ActiveTimeMinutes,
            recipe.SourceUrl,
            recipe.Tags.Select(tag => tag.Value).ToArray(),
            [],
            recipe.Ingredients.Select(ToIngredientResponse).ToArray(),
            recipe.Steps.Select(step => new PreparationStepResponse(step.Number, step.Text)).ToArray());

    private static IngredientResponse ToIngredientResponse(RecipeIngredient ingredient) =>
        new(
            ingredient.IngredientId,
            ingredient.Name.Value,
            ingredient.Quantity.Amount,
            ingredient.Quantity.Unit.ToString(),
            ingredient.Comment,
            ingredient.IsOptional,
            ingredient.Category.ToString());
}
