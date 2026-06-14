using System.Text.RegularExpressions;

namespace MenuMate.Modules.RecipeImports.Application.Extraction;

/// <summary>
/// Подготавливает замечания распознавания для показа пользователю.
/// </summary>
public static partial class RecipeImportWarningNormalizer
{
    private const int MaxWarnings = 6;
    private const int MaxWarningLength = 240;
    private const string GenericWarning =
        "Проверьте распознанные данные: часть информации на изображении прочитана неуверенно.";

    /// <summary>Удаляет технические детали и ограничивает количество замечаний.</summary>
    public static IReadOnlyCollection<string> Normalize(IEnumerable<string>? warnings)
    {
        if (warnings is null)
        {
            return [];
        }

        var result = new List<string>(MaxWarnings);
        var uniqueWarnings = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (string warning in warnings)
        {
            string? normalized = NormalizeWarning(warning);
            if (normalized is null || !uniqueWarnings.Add(normalized))
            {
                continue;
            }

            result.Add(normalized);
            if (result.Count == MaxWarnings)
            {
                break;
            }
        }

        return result;
    }

    private static string? NormalizeWarning(string warning)
    {
        if (string.IsNullOrWhiteSpace(warning))
        {
            return null;
        }

        string normalized = ListPrefixRegex().Replace(warning.Trim(), string.Empty);
        normalized = WhitespaceRegex().Replace(normalized, " ").Trim();

        if (TechnicalDetailsRegex().IsMatch(normalized) ||
            (LatinLetterRegex().IsMatch(normalized) && !CyrillicLetterRegex().IsMatch(normalized)))
        {
            return GenericWarning;
        }

        if (normalized.Length > MaxWarningLength)
        {
            normalized = string.Concat(normalized.AsSpan(0, MaxWarningLength - 1).TrimEnd(), "…");
        }

        return normalized;
    }

    [GeneratedRegex(@"^(?:[-*•]\s*|\d+[.)]\s*)", RegexOptions.CultureInvariant)]
    private static partial Regex ListPrefixRegex();

    [GeneratedRegex(@"\s+", RegexOptions.CultureInvariant)]
    private static partial Regex WhitespaceRegex();

    [GeneratedRegex(
        @"\b(?:json|schema|enum|null|unknown|other|confidence|payload|response|property|field|модел[ьи]|схем[аы]|техническ\w*)\b|[a-z][a-z0-9]*\.[a-z][a-z0-9.\[\]]*",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex TechnicalDetailsRegex();

    [GeneratedRegex("[a-z]", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex LatinLetterRegex();

    [GeneratedRegex("[а-яё]", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex CyrillicLetterRegex();
}
