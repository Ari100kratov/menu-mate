using MenuMate.Modules.Recipes.Domain.Enums;
using MenuMate.Modules.Recipes.Domain.Errors;
using MenuMate.SharedKernel;

namespace MenuMate.Modules.Recipes.Domain.ValueObjects;

/// <summary>
/// Количество ингредиента.
/// </summary>
public sealed record IngredientQuantity
{
    private IngredientQuantity(decimal? amount, MeasurementUnit unit)
    {
        Amount = amount;
        Unit = unit;
    }

    /// <summary>
    /// Числовое количество, если оно известно.
    /// </summary>
    public decimal? Amount { get; }

    /// <summary>
    /// Единица измерения.
    /// </summary>
    public MeasurementUnit Unit { get; }

    /// <summary>
    /// Создает числовое количество.
    /// </summary>
    public static Result<IngredientQuantity> Measured(decimal amount, MeasurementUnit unit) =>
        CreateMeasured(amount, unit);

    /// <summary>
    /// Создает количество по вкусу.
    /// </summary>
    public static IngredientQuantity ToTaste() =>
        new(null, MeasurementUnit.ToTaste);

    /// <summary>
    /// Возвращает новую величину, масштабированную под коэффициент персон.
    /// </summary>
    public IngredientQuantity Scale(decimal factor)
    {
        if (Amount is null)
        {
            return this;
        }

        return new IngredientQuantity(Math.Round(Amount.Value * factor, 2), Unit);
    }

    private static Result<IngredientQuantity> CreateMeasured(decimal amount, MeasurementUnit unit)
    {
        if (amount <= 0)
        {
            return Result.Failure<IngredientQuantity>(RecipeErrors.InvalidIngredientAmount);
        }

        return unit == MeasurementUnit.ToTaste
            ? Result.Failure<IngredientQuantity>(RecipeErrors.InvalidIngredientAmount)
            : new IngredientQuantity(amount, unit);
    }
}
