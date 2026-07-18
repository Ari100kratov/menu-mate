using MenuMate.SharedKernel;

namespace MenuMate.Modules.Recipes.Domain.ValueObjects;

/// <summary>
/// Ссылка рецепта на тег из глобального каталога.
/// </summary>
public sealed record RecipeTag
{
    private RecipeTag(Guid id, string value, string normalizedValue)
    {
        Id = id;
        Value = value;
        NormalizedValue = normalizedValue;
    }

    /// <summary>Идентификатор глобального тега.</summary>
    public Guid Id { get; }

    /// <summary>Каноническое отображаемое название.</summary>
    public string Value { get; }

    /// <summary>Нормализованное название для сравнения в памяти.</summary>
    public string NormalizedValue { get; }

    /// <summary>Создаёт разрешённую ссылку на глобальный тег.</summary>
    public static Result<RecipeTag> Create(Guid id, string value)
    {
        if (id == Guid.Empty)
        {
            return Result.Failure<RecipeTag>(AppError.Validation(
                "Recipes.EmptyTagId",
                "Идентификатор тега не может быть пустым."));
        }

        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<RecipeTag>(AppError.Validation(
                "Recipes.EmptyTag",
                "Тег не может быть пустым."));
        }

        string normalizedValue = TextNormalizer.NormalizeSearchText(value);
        if (normalizedValue.Length > 64)
        {
            return Result.Failure<RecipeTag>(AppError.Validation(
                "Recipes.TagTooLong",
                "Название тега не может быть длиннее 64 символов."));
        }

        return new RecipeTag(id, value.Trim(), normalizedValue);
    }
}
