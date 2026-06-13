using MenuMate.Modules.Recipes.Domain.Enums;
using MenuMate.Modules.Recipes.Domain.Models;
using MenuMate.Modules.Recipes.Domain.ValueObjects;

namespace MenuMate.Modules.Recipes.Domain.UnitTests.Models;

public sealed class RecipeIngredientTests
{
    [Fact]
    public void ScaleShouldKeepMetadataAndScaleQuantity()
    {
        var ingredientId = Guid.CreateVersion7();
        var ingredient = new RecipeIngredient(
            ingredientId,
            IngredientName.Create("Рис").Value,
            IngredientQuantity.Measured(100m, MeasurementUnit.Gram).Value,
            ProductCategory.GrainsAndPasta,
            "промыть",
            true);

        RecipeIngredient scaled = ingredient.Scale(2.5m);

        Assert.Equal(ingredientId, scaled.IngredientId);
        Assert.Equal(250m, scaled.Quantity.Amount);
        Assert.Equal("промыть", scaled.Comment);
        Assert.True(scaled.IsOptional);
    }
}
