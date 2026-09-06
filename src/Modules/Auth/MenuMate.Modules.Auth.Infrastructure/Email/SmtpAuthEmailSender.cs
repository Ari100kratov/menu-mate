using System.Globalization;
using System.Net;
using MailKit.Net.Smtp;
using MailKit.Security;
using MenuMate.Modules.Auth.Application.Abstractions;
using Microsoft.Extensions.Options;
using MimeKit;

namespace MenuMate.Modules.Auth.Infrastructure.Email;

internal sealed class SmtpAuthEmailSender(IOptions<EmailOptions> options) : IAuthEmailSender
{
    private readonly EmailOptions _options = options.Value;

    public Task SendVerificationCodeAsync(
        string email,
        string code,
        DateTimeOffset expiresAt,
        CancellationToken cancellationToken) =>
        SendAsync(
            email,
            "Код подтверждения MenuMate",
            $"Код подтверждения: {code}\nКод действует до {FormatTime(expiresAt)}.",
            $"<p>Код подтверждения:</p><p style=\"font-size:28px;font-weight:700;letter-spacing:6px\">{WebUtility.HtmlEncode(code)}</p><p>Код действует до {FormatTime(expiresAt)}.</p>",
            cancellationToken);

    public Task SendEmailChangeCodeAsync(
        string email,
        string code,
        DateTimeOffset expiresAt,
        CancellationToken cancellationToken) =>
        SendAsync(
            email,
            "Подтверждение нового email в MenuMate",
            $"Код подтверждения нового email: {code}\nКод действует до {FormatTime(expiresAt)}.",
            $"<p>Код подтверждения нового email:</p><p style=\"font-size:28px;font-weight:700;letter-spacing:6px\">{WebUtility.HtmlEncode(code)}</p><p>Код действует до {FormatTime(expiresAt)}.</p>",
            cancellationToken);

    public Task SendPasswordResetAsync(
        string email,
        Uri resetUrl,
        DateTimeOffset expiresAt,
        CancellationToken cancellationToken) =>
        SendAsync(
            email,
            "Сброс пароля MenuMate",
            $"Чтобы задать новый пароль, откройте ссылку: {resetUrl.AbsoluteUri}\nСсылка действует до {FormatTime(expiresAt)}.",
            $"<p>Чтобы задать новый пароль, перейдите по ссылке:</p><p><a href=\"{WebUtility.HtmlEncode(resetUrl.AbsoluteUri)}\">Сбросить пароль</a></p><p>Ссылка действует до {FormatTime(expiresAt)}.</p>",
            cancellationToken);

    public Task SendSecurityNotificationAsync(
        string email,
        string subject,
        string message,
        CancellationToken cancellationToken) =>
        SendAsync(
            email,
            subject,
            message,
            $"<p>{WebUtility.HtmlEncode(message)}</p>",
            cancellationToken);

    private async Task SendAsync(
        string recipient,
        string subject,
        string text,
        string html,
        CancellationToken cancellationToken)
    {
        if (!_options.Enabled)
        {
            throw new AuthEmailDeliveryException("Email delivery is disabled.");
        }

        using var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_options.FromName, _options.FromAddress));
        message.To.Add(MailboxAddress.Parse(recipient));
        message.Subject = subject;
        message.Body = new BodyBuilder { TextBody = text, HtmlBody = html }.ToMessageBody();

        try
        {
            using var client = new SmtpClient();
            await client.ConnectAsync(
                _options.Smtp.Host,
                _options.Smtp.Port,
                ParseSecurity(_options.Smtp.Security),
                cancellationToken);

            if (!string.IsNullOrWhiteSpace(_options.Smtp.Username))
            {
                await client.AuthenticateAsync(
                    _options.Smtp.Username,
                    _options.Smtp.Password,
                    cancellationToken);
            }

            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(quit: true, cancellationToken);
        }
        catch (Exception exception) when (
            exception is MailKit.Security.AuthenticationException or
            MailKit.ServiceNotAuthenticatedException or
            MailKit.ServiceNotConnectedException or
            SmtpCommandException or SmtpProtocolException or IOException or
            System.Net.Sockets.SocketException or
            System.Security.Authentication.AuthenticationException)
        {
            throw new AuthEmailDeliveryException("SMTP delivery failed.", exception);
        }
    }

    private static SecureSocketOptions ParseSecurity(string value) => value.ToUpperInvariant() switch
    {
        "NONE" => SecureSocketOptions.None,
        "STARTTLS" => SecureSocketOptions.StartTls,
        "SSLONCONNECT" => SecureSocketOptions.SslOnConnect,
        _ => throw new InvalidOperationException(
            "Email:Smtp:Security must be None, StartTls or SslOnConnect.")
    };

    private static string FormatTime(DateTimeOffset value) =>
        value.ToString("dd.MM.yyyy HH:mm 'UTC'", CultureInfo.InvariantCulture);
}
