using MenuMate.Modules.Recipes.Domain.ValueObjects;
using MenuMate.SharedKernel;

namespace MenuMate.Modules.Recipes.Domain.UnitTests.ValueObjects;

public sealed class RecipeTagTests
{
    [Fact]
    public void CreateShouldTrimAndNormalizeTag()
    {
        RecipeTag tag = RecipeTag.Create("  Быстрый   ужин ").Value;

        Assert.Equal("Быстрый   ужин", tag.Value);
        Assert.Equal("БЫСТРЫЙ УЖИН", tag.NormalizedValue);
    }

    [Fact]
    public void CreateShouldRejectEmptyTag()
    {
        Result<RecipeTag> result = RecipeTag.Create(" ");

        Assert.True(result.IsFailure);
        Assert.Equal("Recipes.EmptyTag", result.Error.Code);
    }
}
