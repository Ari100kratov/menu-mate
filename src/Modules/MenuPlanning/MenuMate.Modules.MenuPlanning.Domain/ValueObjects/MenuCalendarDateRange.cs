using MenuMate.Modules.MenuPlanning.Domain.Errors;
using MenuMate.SharedKernel;

namespace MenuMate.Modules.MenuPlanning.Domain.ValueObjects;

/// <summary>
/// Диапазон дат для чтения календаря меню.
/// </summary>
public sealed record MenuCalendarDateRange
{
    /// <summary>
    /// Максимальная длина диапазона в днях.
    /// </summary>
    public const int MaxDays = 366;

    private MenuCalendarDateRange(DateOnly startDate, DateOnly endDate)
    {
        StartDate = startDate;
        EndDate = endDate;
    }

    /// <summary>
    /// Дата начала.
    /// </summary>
    public DateOnly StartDate { get; }

    /// <summary>
    /// Дата окончания.
    /// </summary>
    public DateOnly EndDate { get; }

    /// <summary>
    /// Создает диапазон меню.
    /// </summary>
    public static Result<MenuCalendarDateRange> Create(DateOnly startDate, DateOnly endDate)
    {
        if (endDate < startDate)
        {
            return Result.Failure<MenuCalendarDateRange>(MenuCalendarErrors.InvalidDateRange);
        }

        if (endDate.DayNumber - startDate.DayNumber + 1 > MaxDays)
        {
            return Result.Failure<MenuCalendarDateRange>(MenuCalendarErrors.DateRangeTooLong);
        }

        return new MenuCalendarDateRange(startDate, endDate);
    }

    /// <summary>
    /// Возвращает true, если дата входит в диапазон.
    /// </summary>
    public bool Contains(DateOnly date) => date >= StartDate && date <= EndDate;
}
