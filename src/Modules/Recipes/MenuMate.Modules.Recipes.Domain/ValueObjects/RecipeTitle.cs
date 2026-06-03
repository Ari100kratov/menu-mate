using MenuMate.Modules.Recipes.Domain.Errors;
using MenuMate.SharedKernel;

namespace MenuMate.Modules.Recipes.Domain.ValueObjects;

/// <summary>
/// Название рецепта.
/// </summary>
public sealed record RecipeTitle
{
    private RecipeTitle(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Отображаемое название.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Создает название рецепта.
    /// </summary>
    public static Result<RecipeTitle> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<RecipeTitle>(RecipeErrors.EmptyTitle);
        }

        string trimmed = value.Trim();
        if (trimmed.Length > 160)
        {
            return Result.Failure<RecipeTitle>(RecipeErrors.TitleTooLong);
        }

        return new RecipeTitle(trimmed);
    }

    /// <inheritdoc />
    public override string ToString() => Value;
}

