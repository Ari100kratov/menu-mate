using MenuMate.Contracts.Recipes;
using MenuMate.Modules.Recipes.Domain.Models;

namespace MenuMate.Modules.Recipes.Application;

internal static class RecipeMapping
{
    public static RecipeResponse ToResponse(Recipe recipe) =>
        new(
            recipe.Id,
            recipe.Title.Value,
            recipe.Description,
            recipe.Servings.Value,
            recipe.IsFavorite,
            recipe.SourceUrl,
            recipe.Tags.Select(tag => tag.Value).ToArray(),
            [],
            recipe.Ingredients.Select(ToIngredientResponse).ToArray(),
            recipe.Steps.Select(step => new PreparationStepResponse(step.Number, step.Text)).ToArray());

    private static IngredientResponse ToIngredientResponse(RecipeIngredient ingredient) =>
        new(
            ingredient.Name.Value,
            ingredient.Quantity.Amount,
            ingredient.Quantity.Unit.ToString(),
            ingredient.Comment,
            ingredient.IsOptional,
            ingredient.Quantity.Kind.ToString(),
            ingredient.Category.ToString());
}
