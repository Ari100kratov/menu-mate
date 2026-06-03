using MenuMate.SharedKernel;

namespace MenuMate.Modules.Recipes.Domain.ValueObjects;

/// <summary>
/// Снимок тега внутри рецепта.
/// </summary>
public sealed record RecipeTag
{
    private RecipeTag(string value, string normalizedValue)
    {
        Value = value;
        NormalizedValue = normalizedValue;
    }

    /// <summary>
    /// Отображаемое название тега.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Нормализованное название тега.
    /// </summary>
    public string NormalizedValue { get; }

    /// <summary>
    /// Создает тег рецепта.
    /// </summary>
    public static Result<RecipeTag> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<RecipeTag>(AppError.Validation("Recipes.EmptyTag", "Тег не может быть пустым."));
        }

        return new RecipeTag(value.Trim(), TextNormalizer.NormalizeSearchText(value));
    }
}


