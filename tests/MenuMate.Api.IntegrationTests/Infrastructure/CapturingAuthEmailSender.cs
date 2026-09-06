using System.Collections.Concurrent;
using MenuMate.Modules.Auth.Application.Abstractions;

namespace MenuMate.Api.IntegrationTests;

internal sealed class CapturingAuthEmailSender : IAuthEmailSender
{
    private static readonly ConcurrentDictionary<string, string> VerificationCodes = new();
    private static readonly ConcurrentDictionary<string, string> EmailChangeCodes = new();
    private static readonly ConcurrentDictionary<string, Uri> PasswordResetUrls = new();

    public Task SendVerificationCodeAsync(
        string email,
        string code,
        DateTimeOffset expiresAt,
        CancellationToken cancellationToken)
    {
        if (email.Contains("delivery-failure", StringComparison.Ordinal))
        {
            throw new AuthEmailDeliveryException("Simulated delivery failure.");
        }

        VerificationCodes[email] = code;
        return Task.CompletedTask;
    }

    public Task SendEmailChangeCodeAsync(
        string email,
        string code,
        DateTimeOffset expiresAt,
        CancellationToken cancellationToken)
    {
        EmailChangeCodes[email] = code;
        return Task.CompletedTask;
    }

    public Task SendPasswordResetAsync(
        string email,
        Uri resetUrl,
        DateTimeOffset expiresAt,
        CancellationToken cancellationToken)
    {
        PasswordResetUrls[email] = resetUrl;
        return Task.CompletedTask;
    }

    public Task SendSecurityNotificationAsync(
        string email,
        string subject,
        string message,
        CancellationToken cancellationToken) => Task.CompletedTask;

    public static string GetVerificationCode(string email) => VerificationCodes[email];

    public static string GetEmailChangeCode(string email) => EmailChangeCodes[email];

    public static string GetPasswordResetToken(string email)
    {
        Uri url = PasswordResetUrls[email];
        string fragment = url.Fragment.TrimStart('#');
        string tokenPair = fragment.Split('&', StringSplitOptions.RemoveEmptyEntries)
            .Single(part => part.StartsWith("token=", StringComparison.Ordinal));
        return Uri.UnescapeDataString(tokenPair["token=".Length..]);
    }

    public static bool HasPasswordReset(string email) => PasswordResetUrls.ContainsKey(email);
}
