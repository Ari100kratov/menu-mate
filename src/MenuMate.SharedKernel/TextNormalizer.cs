namespace MenuMate.SharedKernel;

/// <summary>
/// Общие правила нормализации коротких пользовательских строк для поиска и дедупликации.
/// </summary>
public static class TextNormalizer
{
    /// <summary>
    /// Убирает лишние пробелы и приводит строку к верхнему регистру.
    /// </summary>
    public static string NormalizeSearchText(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        string[] parts = value.Trim().ToUpperInvariant().Split(
            separator: (char[]?)null,
            options: StringSplitOptions.RemoveEmptyEntries);

        return string.Join(' ', parts);
    }

    /// <summary>
    /// Нормализует пользовательский поисковый запрос и считает буквы «ё» и «е» эквивалентными.
    /// </summary>
    public static string NormalizeSearchQuery(string value) =>
        NormalizeSearchText(value).Replace('Ё', 'Е');
}
