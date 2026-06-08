using MenuMate.Contracts.Recipes;
using MenuMate.Modules.Recipes.Domain.Models;
using MenuMate.SharedKernel.Identifiers;

namespace MenuMate.Modules.Recipes.Application;

internal static class RecipeMapping
{
    public static RecipeResponse ToResponse(
        Recipe recipe,
        UserId currentUserId,
        bool isSaved,
        bool isFavorite) =>
        new(
            recipe.Id,
            recipe.CurrentRevisionId.Value,
            recipe.RevisionNumber,
            recipe.OwnerUserId == currentUserId,
            isSaved,
            recipe.SourceRecipeId,
            recipe.SourceRevisionId?.Value,
            recipe.Title.Value,
            recipe.Description,
            recipe.Servings.Value,
            recipe.Category.ToString(),
            recipe.Visibility.ToString(),
            recipe.TotalTimeMinutes,
            recipe.ActiveTimeMinutes,
            isFavorite,
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
            ingredient.Quantity.Kind.ToString(),
            ingredient.Category.ToString());
}
