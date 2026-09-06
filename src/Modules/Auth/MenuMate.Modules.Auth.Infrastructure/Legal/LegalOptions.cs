namespace MenuMate.Modules.Auth.Infrastructure.Legal;

internal sealed class LegalOptions
{
    public const string SectionName = "Legal";

    public string OperatorName { get; init; } = string.Empty;
    public string PrivacyContactEmail { get; init; } = string.Empty;
    public int TechnicalLogRetentionDays { get; init; } = 30;
    public int BackupRetentionDays { get; init; } = 30;
}
