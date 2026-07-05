using Microsoft.Extensions.Configuration;

namespace MenuMate.DataImporter;

internal sealed record ImportOptions(
    string AdminEmail,
    string? ConnectionString,
    int? MaxItems,
    bool DryRun,
    bool Resume,
    bool SkipImages,
    string SourceCategory,
    string UserAgent,
    int RequestDelayMilliseconds,
    Uri ApiUrl,
    Uri SiteUrl)
{
    public static ImportOptions Create(IConfiguration configuration, string[] args)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(args);

        Dictionary<string, string?> values = ParseArguments(args);
        string adminEmail = GetValue(values, "admin-email") ?? configuration["DataImport:AdminEmail"] ?? string.Empty;
        string sourceCategory = GetValue(values, "source-category") ??
            configuration["DataImport:SourceCategory"] ??
            "Рецепты";
        string userAgent = configuration["DataImport:UserAgent"] ?? "MenuMateDataImporter/1.0";
        int requestDelay = int.TryParse(
            configuration["DataImport:RequestDelayMilliseconds"],
            out int parsedDelay)
            ? Math.Max(parsedDelay, 0)
            : 200;

        return new ImportOptions(
            adminEmail.Trim(),
            GetValue(values, "connection-string"),
            ParsePositiveInt(GetValue(values, "max-items")),
            values.ContainsKey("dry-run"),
            values.ContainsKey("resume"),
            values.ContainsKey("skip-images"),
            sourceCategory.Trim(),
            userAgent.Trim(),
            requestDelay,
            new Uri(configuration["DataImport:ApiUrl"] ??
                throw new InvalidOperationException("DataImport:ApiUrl is not configured."), UriKind.Absolute),
            new Uri(configuration["DataImport:SiteUrl"] ??
                throw new InvalidOperationException("DataImport:SiteUrl is not configured."), UriKind.Absolute));
    }

    private static Dictionary<string, string?> ParseArguments(string[] args)
    {
        var values = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        int index = 0;
        while (index < args.Length)
        {
            string argument = args[index];
            if (!argument.StartsWith("--", StringComparison.Ordinal))
            {
                index++;
                continue;
            }

            string key = argument[2..];
            string? value = index + 1 < args.Length && !args[index + 1].StartsWith("--", StringComparison.Ordinal)
                ? args[index + 1]
                : null;
            values[key] = value;
            index += value is null ? 1 : 2;
        }

        return values;
    }

    private static string? GetValue(Dictionary<string, string?> values, string key) =>
        values.TryGetValue(key, out string? value) ? value : null;

    private static int? ParsePositiveInt(string? value) =>
        int.TryParse(value, out int parsed) && parsed > 0 ? parsed : null;
}
