using MenuMate.Modules.Recipes.Domain.Enums;
using MenuMate.Modules.Recipes.Domain.Errors;
using MenuMate.SharedKernel;

namespace MenuMate.Modules.Recipes.Domain.ValueObjects;

/// <summary>
/// Количество ингредиента.
/// </summary>
public sealed record IngredientQuantity
{
    private IngredientQuantity(decimal? amount, MeasurementUnit unit, IngredientQuantityKind kind)
    {
        Amount = amount;
        Unit = unit;
        Kind = kind;
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
    /// Тип количества.
    /// </summary>
    public IngredientQuantityKind Kind { get; }

    /// <summary>
    /// Создает точное количество.
    /// </summary>
    public static Result<IngredientQuantity> Exact(decimal amount, MeasurementUnit unit) =>
        CreateMeasured(amount, unit, IngredientQuantityKind.Exact);

    /// <summary>
    /// Создает примерное количество.
    /// </summary>
    public static Result<IngredientQuantity> Approximate(decimal amount, MeasurementUnit unit) =>
        CreateMeasured(amount, unit, IngredientQuantityKind.Approximate);

    /// <summary>
    /// Создает количество по вкусу.
    /// </summary>
    public static IngredientQuantity ToTaste() =>
        new(null, MeasurementUnit.ToTaste, IngredientQuantityKind.ToTaste);

    /// <summary>
    /// Возвращает новую величину, масштабированную под коэффициент персон.
    /// </summary>
    public IngredientQuantity Scale(decimal factor)
    {
        if (Amount is null)
        {
            return this;
        }

        return new IngredientQuantity(Math.Round(Amount.Value * factor, 2), Unit, Kind);
    }

    private static Result<IngredientQuantity> CreateMeasured(
        decimal amount,
        MeasurementUnit unit,
        IngredientQuantityKind kind)
    {
        if (amount <= 0)
        {
            return Result.Failure<IngredientQuantity>(RecipeErrors.InvalidIngredientAmount);
        }

        return new IngredientQuantity(amount, unit, kind);
    }
}

