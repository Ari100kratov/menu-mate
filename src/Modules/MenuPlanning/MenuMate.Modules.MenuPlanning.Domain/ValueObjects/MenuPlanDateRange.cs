using MenuMate.Modules.MenuPlanning.Domain.Errors;
using MenuMate.SharedKernel;

namespace MenuMate.Modules.MenuPlanning.Domain.ValueObjects;

/// <summary>
/// Диапазон дат плана меню.
/// </summary>
public sealed record MenuPlanDateRange
{
    private MenuPlanDateRange(DateOnly startDate, DateOnly endDate)
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
    /// Создает диапазон дат.
    /// </summary>
    public static Result<MenuPlanDateRange> Create(DateOnly startDate, DateOnly endDate)
    {
        if (endDate < startDate)
        {
            return Result.Failure<MenuPlanDateRange>(MenuPlanErrors.InvalidDateRange);
        }

        return new MenuPlanDateRange(startDate, endDate);
    }

    /// <summary>
    /// Проверяет, входит ли дата в диапазон.
    /// </summary>
    public bool Contains(DateOnly date) => date >= StartDate && date <= EndDate;
}

