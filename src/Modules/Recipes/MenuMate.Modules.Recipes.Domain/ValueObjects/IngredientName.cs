using MenuMate.Modules.Recipes.Domain.Errors;
using MenuMate.SharedKernel;

namespace MenuMate.Modules.Recipes.Domain.ValueObjects;

/// <summary>
/// Название продукта с нормализованной формой для суммирования.
/// </summary>
public sealed record IngredientName
{
    private IngredientName(string value, string normalizedValue)
    {
        Value = value;
        NormalizedValue = normalizedValue;
    }

    /// <summary>
    /// Отображаемое название продукта.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Нормализованное название продукта.
    /// </summary>
    public string NormalizedValue { get; }

    /// <summary>
    /// Создает название продукта.
    /// </summary>
    public static Result<IngredientName> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<IngredientName>(RecipeErrors.EmptyIngredientName);
        }

        string normalized = TextNormalizer.NormalizeSearchText(value);
        return new IngredientName(value.Trim(), normalized);
    }

    /// <inheritdoc />
    public override string ToString() => Value;
}

