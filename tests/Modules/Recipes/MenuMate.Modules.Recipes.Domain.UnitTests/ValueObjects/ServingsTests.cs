using MenuMate.Modules.Recipes.Domain.Errors;
using MenuMate.Modules.Recipes.Domain.ValueObjects;
using MenuMate.SharedKernel;

namespace MenuMate.Modules.Recipes.Domain.UnitTests.ValueObjects;

public sealed class ServingsTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    public void CreateShouldAcceptBoundaryValues(int value)
    {
        Servings servings = Servings.Create(value).Value;

        Assert.Equal(value, servings.Value);
        Assert.Equal(value.ToString(System.Globalization.CultureInfo.InvariantCulture), servings.ToString());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public void CreateShouldRejectValuesOutsideRange(int value)
    {
        Result<Servings> result = Servings.Create(value);

        Assert.True(result.IsFailure);
        Assert.Equal(RecipeErrors.InvalidServings, result.Error);
    }
}
