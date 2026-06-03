using MenuMate.Modules.Tags.Domain.Errors;
using MenuMate.SharedKernel;

namespace MenuMate.Modules.Tags.Domain.ValueObjects;

/// <summary>
/// Название тега с нормализованной формой для поиска.
/// </summary>
public sealed record TagName
{
    private TagName(string value, string normalizedValue)
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
    /// Создает название тега с проверкой длины и пустого значения.
    /// </summary>
    public static Result<TagName> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<TagName>(TagErrors.EmptyName);
        }

        string normalized = TextNormalizer.NormalizeSearchText(value);
        if (normalized.Length > 64)
        {
            return Result.Failure<TagName>(TagErrors.NameTooLong);
        }

        return new TagName(value.Trim(), normalized);
    }

    /// <inheritdoc />
    public override string ToString() => Value;
}

