using System.Globalization;
using MenuMate.Modules.Recipes.Domain.Errors;
using MenuMate.SharedKernel;

namespace MenuMate.Modules.Recipes.Domain.ValueObjects;

/// <summary>
/// Количество персон, на которое рассчитан рецепт.
/// </summary>
public readonly record struct Servings
{
    private Servings(int value)
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
    public static Result<Servings> Create(int value)
    {
        if (value is < 1 or > 100)
        {
            return Result.Failure<Servings>(RecipeErrors.InvalidServings);
        }

        return new Servings(value);
    }

    /// <inheritdoc />
    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
}
