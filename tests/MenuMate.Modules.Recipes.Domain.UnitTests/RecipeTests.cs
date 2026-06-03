using MenuMate.Modules.Recipes.Domain.Enums;
using MenuMate.Modules.Recipes.Domain.Models;
using MenuMate.Modules.Recipes.Domain.ValueObjects;
using MenuMate.SharedKernel.Identifiers;

namespace MenuMate.Modules.Recipes.Domain.UnitTests;

public sealed class RecipeTests
{
    private static readonly DateTimeOffset FixedNow = new(2026, 5, 30, 12, 0, 0, TimeSpan.Zero);
    private static readonly UserId OwnerUserId = UserId.Create();

    [Fact]
    public void ScaleIngredientsForShouldNotChangeSourceRecipe()
    {
        var recipe = Recipe.Create(
            Guid.CreateVersion7(),
            OwnerUserId,
            RecipeTitle.Create("Oatmeal").Value,
            Servings.Create(2).Value,
            FixedNow);

        var ingredient = new RecipeIngredient(
            IngredientName.Create("Milk").Value,
            IngredientQuantity.Exact(250m, MeasurementUnit.Milliliter).Value,
            ProductCategory.Dairy,
            null,
            false);

        recipe.ReplaceIngredients([ingredient], FixedNow);

        RecipeIngredient scaledIngredient = recipe.ScaleIngredientsFor(Servings.Create(4).Value).Single();

        Assert.Equal(500m, scaledIngredient.Quantity.Amount);
        Assert.Equal(250m, recipe.Ingredients.Single().Quantity.Amount);
    }

    [Fact]
    public void AddTagShouldDeduplicateByNormalizedName()
    {
        var recipe = Recipe.Create(
            Guid.CreateVersion7(),
            OwnerUserId,
            RecipeTitle.Create("Pasta").Value,
            Servings.Create(2).Value,
            FixedNow);

        recipe.AddTag(RecipeTag.Create(" Fast ").Value, FixedNow);
        recipe.AddTag(RecipeTag.Create("fast").Value, FixedNow);

        Assert.Single(recipe.Tags);
        Assert.Equal("FAST", recipe.Tags.Single().NormalizedValue);
    }

    [Fact]
    public void ScaleIngredientsForShouldKeepTextualQuantity()
    {
        var recipe = Recipe.Create(
            Guid.CreateVersion7(),
            OwnerUserId,
            RecipeTitle.Create("Salad").Value,
            Servings.Create(2).Value,
            FixedNow);

        var ingredient = new RecipeIngredient(
            IngredientName.Create("Salt").Value,
            IngredientQuantity.ToTaste(),
            ProductCategory.Spices,
            "to taste",
            false);

        recipe.ReplaceIngredients([ingredient], FixedNow);

        RecipeIngredient scaledIngredient = recipe.ScaleIngredientsFor(Servings.Create(6).Value).Single();

        Assert.Null(scaledIngredient.Quantity.Amount);
        Assert.Equal(MeasurementUnit.ToTaste, scaledIngredient.Quantity.Unit);
    }
}
