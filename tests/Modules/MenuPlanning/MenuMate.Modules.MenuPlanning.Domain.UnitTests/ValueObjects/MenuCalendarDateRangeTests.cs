using MenuMate.Modules.MenuPlanning.Domain.Errors;
using MenuMate.Modules.MenuPlanning.Domain.ValueObjects;
using MenuMate.SharedKernel;

namespace MenuMate.Modules.MenuPlanning.Domain.UnitTests.ValueObjects;

public sealed class MenuCalendarDateRangeTests
{
    [Fact]
    public void CreateShouldIncludeBoundaryDates()
    {
        DateOnly start = new(2026, 6, 1);
        DateOnly end = new(2026, 6, 7);
        MenuCalendarDateRange range = MenuCalendarDateRange.Create(start, end).Value;

        Assert.True(range.Contains(start));
        Assert.True(range.Contains(end));
        Assert.False(range.Contains(end.AddDays(1)));
    }

    [Fact]
    public void CreateShouldRejectReversedRange()
    {
        Result<MenuCalendarDateRange> result = MenuCalendarDateRange.Create(
            new DateOnly(2026, 6, 2),
            new DateOnly(2026, 6, 1));

        Assert.True(result.IsFailure);
        Assert.Equal(MenuCalendarErrors.InvalidDateRange, result.Error);
    }

    [Fact]
    public void CreateShouldRejectRangeLongerThanMaximum()
    {
        DateOnly start = new(2026, 1, 1);

        Result<MenuCalendarDateRange> result = MenuCalendarDateRange.Create(
            start,
            start.AddDays(MenuCalendarDateRange.MaxDays));

        Assert.True(result.IsFailure);
        Assert.Equal(MenuCalendarErrors.DateRangeTooLong, result.Error);
    }
}
