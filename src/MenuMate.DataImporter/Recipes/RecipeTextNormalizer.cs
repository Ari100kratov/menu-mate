using System.Text.RegularExpressions;

namespace MenuMate.DataImporter.Recipes;

internal static partial class RecipeTextNormalizer
{
    public static string NormalizeStep(string value) =>
        StepPrefixRegex().Replace(CleanWikiMarkup(value), string.Empty).Trim();

    public static string CleanWikiMarkup(string value)
    {
        string result = LinkWithLabelRegex().Replace(value, "$1");
        result = SimpleLinkRegex().Replace(result, "$1");
        result = TemplateRegex().Replace(result, string.Empty);
        result = FormattingRegex().Replace(result, string.Empty);
        return result.Trim(' ', '\t', '-', '*', '#', ':');
    }

    [GeneratedRegex(@"^\s*(?:(?:(?:шаг|step)\s*)?\d+\s*[.)\]:\-–—]\s*|(?:шаг|step)\s*\d+\s+)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex StepPrefixRegex();

    [GeneratedRegex(@"\[\[[^|\]]+\|([^\]]+)\]\]", RegexOptions.CultureInvariant)]
    private static partial Regex LinkWithLabelRegex();

    [GeneratedRegex(@"\[\[([^\]]+)\]\]", RegexOptions.CultureInvariant)]
    private static partial Regex SimpleLinkRegex();

    [GeneratedRegex(@"\{\{[^{}]*\}\}", RegexOptions.CultureInvariant)]
    private static partial Regex TemplateRegex();

    [GeneratedRegex(@"'{2,}", RegexOptions.CultureInvariant)]
    private static partial Regex FormattingRegex();
}
