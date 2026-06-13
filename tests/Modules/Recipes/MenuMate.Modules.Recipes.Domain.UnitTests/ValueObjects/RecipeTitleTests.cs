using MenuMate.Modules.Recipes.Domain.Errors;
using MenuMate.Modules.Recipes.Domain.ValueObjects;
using MenuMate.SharedKernel;

namespace MenuMate.Modules.Recipes.Domain.UnitTests.ValueObjects;

public sealed class RecipeTitleTests
{
    [Fact]
    public void CreateShouldTrimTitle()
    {
        RecipeTitle title = RecipeTitle.Create("  Паста  ").Value;

        Assert.Equal("Паста", title.Value);
        Assert.Equal("Паста", title.ToString());
    }

    [Fact]
    public void CreateShouldRejectEmptyTitle()
    {
        Result<RecipeTitle> result = RecipeTitle.Create(" ");

        Assert.True(result.IsFailure);
        Assert.Equal(RecipeErrors.EmptyTitle, result.Error);
    }

    [Fact]
    public void CreateShouldRejectTitleLongerThanLimit()
    {
        Result<RecipeTitle> result = RecipeTitle.Create(new string('а', 161));

        Assert.True(result.IsFailure);
        Assert.Equal(RecipeErrors.TitleTooLong, result.Error);
    }
}
