using MenuMate.Modules.Recipes.Domain.Enums;
using MenuMate.Modules.Recipes.Domain.Errors;
using MenuMate.Modules.Recipes.Domain.ValueObjects;
using MenuMate.SharedKernel;

namespace MenuMate.Modules.Recipes.Domain.UnitTests.ValueObjects;

public sealed class IngredientQuantityTests
{
    [Fact]
    public void MeasuredShouldCreatePositiveQuantity()
    {
        IngredientQuantity quantity = IngredientQuantity.Measured(1.5m, MeasurementUnit.Kilogram).Value;

        Assert.Equal(1.5m, quantity.Amount);
        Assert.Equal(MeasurementUnit.Kilogram, quantity.Unit);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void MeasuredShouldRejectNonPositiveAmount(decimal amount)
    {
        Result<IngredientQuantity> result = IngredientQuantity.Measured(amount, MeasurementUnit.Gram);

        Assert.True(result.IsFailure);
        Assert.Equal(RecipeErrors.InvalidIngredientAmount, result.Error);
    }

    [Fact]
    public void MeasuredShouldRejectToTasteUnit()
    {
        Result<IngredientQuantity> result = IngredientQuantity.Measured(1m, MeasurementUnit.ToTaste);

        Assert.True(result.IsFailure);
        Assert.Equal(RecipeErrors.InvalidIngredientAmount, result.Error);
    }

    [Fact]
    public void ScaleShouldRoundMeasuredAmountAndKeepToTaste()
    {
        IngredientQuantity measured = IngredientQuantity.Measured(1m, MeasurementUnit.Gram).Value;
        var toTaste = IngredientQuantity.ToTaste();

        Assert.Equal(0.33m, measured.Scale(0.333m).Amount);
        Assert.Same(toTaste, toTaste.Scale(4m));
    }
}
