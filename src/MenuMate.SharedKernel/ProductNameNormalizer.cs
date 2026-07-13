namespace MenuMate.SharedKernel;

/// <summary>
/// Приводит названия продуктов к единому виду для хранения и поиска.
/// </summary>
public static class ProductNameNormalizer
{
    /// <summary>
    /// Убирает лишние пробелы, приводит текст к нижнему регистру и заменяет «ё» на «е».
    /// </summary>
    public static string Normalize(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

#pragma warning disable CA1308 // Имена продуктов намеренно хранятся в нижнем регистре.
        string[] parts = value
            .Trim()
            .Replace('ё', 'е')
            .Replace('Ё', 'Е')
            .ToLowerInvariant()
            .Split(separator: (char[]?)null, options: StringSplitOptions.RemoveEmptyEntries);
#pragma warning restore CA1308

        return string.Join(' ', parts);
    }

    /// <summary>
    /// Возвращает технический ключ для регистронезависимого сравнения названий продуктов.
    /// </summary>
    public static string NormalizeForComparison(string value) =>
        TextNormalizer.NormalizeSearchText(Normalize(value));
}
