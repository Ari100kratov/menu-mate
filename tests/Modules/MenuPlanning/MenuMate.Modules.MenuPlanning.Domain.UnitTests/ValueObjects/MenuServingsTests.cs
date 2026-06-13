using MenuMate.Modules.MenuPlanning.Domain.Errors;
using MenuMate.Modules.MenuPlanning.Domain.ValueObjects;
using MenuMate.SharedKernel;

namespace MenuMate.Modules.MenuPlanning.Domain.UnitTests.ValueObjects;

public sealed class MenuServingsTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    public void CreateShouldAcceptBoundaryValues(int value)
    {
        MenuServings servings = MenuServings.Create(value).Value;

        Assert.Equal(value, servings.Value);
        Assert.Equal(value.ToString(System.Globalization.CultureInfo.InvariantCulture), servings.ToString());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public void CreateShouldRejectValuesOutsideRange(int value)
    {
        Result<MenuServings> result = MenuServings.Create(value);

        Assert.True(result.IsFailure);
        Assert.Equal(MenuCalendarErrors.InvalidServings, result.Error);
    }
}
