namespace MenuMate.DataImporter.Wikibooks;

internal static class ImageLicensePolicy
{
    public static bool IsAllowed(string? licenseName)
    {
        if (string.IsNullOrWhiteSpace(licenseName))
        {
            return false;
        }

        if (licenseName.Contains("-NC", StringComparison.OrdinalIgnoreCase) ||
            licenseName.Contains("-ND", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return licenseName.Contains("CC BY", StringComparison.OrdinalIgnoreCase) ||
            licenseName.Contains("CC0", StringComparison.OrdinalIgnoreCase) ||
            licenseName.Contains("PUBLIC DOMAIN", StringComparison.OrdinalIgnoreCase) ||
            licenseName.Contains("PD", StringComparison.OrdinalIgnoreCase);
    }
}
