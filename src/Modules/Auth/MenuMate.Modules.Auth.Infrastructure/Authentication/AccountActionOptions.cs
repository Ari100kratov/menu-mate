namespace MenuMate.Modules.Auth.Infrastructure.Authentication;

internal sealed class AccountActionOptions
{
    public const string SectionName = "AccountActions";

    public string HashSecret { get; init; } = string.Empty;
}
