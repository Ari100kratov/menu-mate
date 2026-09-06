namespace MenuMate.Modules.Auth.Infrastructure.Email;

internal sealed class EmailOptions
{
    public const string SectionName = "Email";

    public bool Enabled { get; init; }
    public SmtpOptions Smtp { get; init; } = new();
    public string FromAddress { get; init; } = string.Empty;
    public string FromName { get; init; } = "MenuMate";
}

internal sealed class SmtpOptions
{
    public string Host { get; init; } = string.Empty;
    public int Port { get; init; } = 587;
    public string Security { get; init; } = "StartTls";
    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}
