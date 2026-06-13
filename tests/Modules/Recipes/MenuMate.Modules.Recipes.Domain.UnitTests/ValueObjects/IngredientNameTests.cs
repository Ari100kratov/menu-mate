using MenuMate.Modules.Recipes.Domain.Errors;
using MenuMate.Modules.Recipes.Domain.ValueObjects;
using MenuMate.SharedKernel;

namespace MenuMate.Modules.Recipes.Domain.UnitTests.ValueObjects;

public sealed class IngredientNameTests
{
    [Fact]
    public void CreateShouldTrimAndNormalizeName()
    {
        IngredientName name = IngredientName.Create("  Рис   басмати ").Value;

        Assert.Equal("Рис   басмати", name.Value);
        Assert.Equal("РИС БАСМАТИ", name.NormalizedValue);
    }

    [Fact]
    public void CreateShouldRejectEmptyName()
    {
        Result<IngredientName> result = IngredientName.Create(" ");

        Assert.True(result.IsFailure);
        Assert.Equal(RecipeErrors.EmptyIngredientName, result.Error);
    }
}
