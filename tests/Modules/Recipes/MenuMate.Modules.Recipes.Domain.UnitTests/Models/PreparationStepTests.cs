using MenuMate.Modules.Recipes.Domain.Errors;
using MenuMate.Modules.Recipes.Domain.Models;
using MenuMate.SharedKernel;

namespace MenuMate.Modules.Recipes.Domain.UnitTests.Models;

public sealed class PreparationStepTests
{
    [Fact]
    public void CreateShouldTrimText()
    {
        PreparationStep step = PreparationStep.Create(1, "  Нарезать овощи  ").Value;

        Assert.Equal(1, step.Number);
        Assert.Equal("Нарезать овощи", step.Text);
    }

    [Fact]
    public void CreateShouldRejectEmptyText()
    {
        Result<PreparationStep> result = PreparationStep.Create(1, " ");

        Assert.True(result.IsFailure);
        Assert.Equal(RecipeErrors.EmptyStepText, result.Error);
    }
}
