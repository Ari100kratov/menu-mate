using System.Globalization;
using MenuMate.Modules.MenuPlanning.Domain.Errors;
using MenuMate.SharedKernel;

namespace MenuMate.Modules.MenuPlanning.Domain.ValueObjects;

/// <summary>
/// Количество персон для позиции меню.
/// </summary>
public readonly record struct MenuServings
{
    private MenuServings(int value)
    {
        Value = value;
    }

    /// <summary>
    /// Число персон.
    /// </summary>
    public int Value { get; }

    /// <summary>
    /// Создает количество персон.
    /// </summary>
    public static Result<MenuServings> Create(int value)
    {
        if (value is < 1 or > 100)
        {
            return Result.Failure<MenuServings>(MenuCalendarErrors.InvalidServings);
        }

        return new MenuServings(value);
    }

    /// <inheritdoc />
    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
}
