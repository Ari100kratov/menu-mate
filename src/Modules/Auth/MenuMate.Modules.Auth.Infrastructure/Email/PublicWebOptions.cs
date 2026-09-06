namespace MenuMate.Modules.Auth.Infrastructure.Email;

internal sealed class PublicWebOptions
{
    public const string SectionName = "PublicWeb";

    public string BaseUrl { get; init; } = string.Empty;
}
