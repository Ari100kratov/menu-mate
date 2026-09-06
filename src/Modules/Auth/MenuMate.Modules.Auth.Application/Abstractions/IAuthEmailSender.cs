namespace MenuMate.Modules.Auth.Application.Abstractions;

/// <summary>
/// Отправляет транзакционные письма для сценариев учетной записи.
/// </summary>
public interface IAuthEmailSender
{
    /// <summary>Отправляет код подтверждения текущего email.</summary>
    Task SendVerificationCodeAsync(
        string email,
        string code,
        DateTimeOffset expiresAt,
        CancellationToken cancellationToken);

    /// <summary>Отправляет код подтверждения нового email.</summary>
    Task SendEmailChangeCodeAsync(
        string email,
        string code,
        DateTimeOffset expiresAt,
        CancellationToken cancellationToken);

    /// <summary>Отправляет ссылку для сброса пароля.</summary>
    Task SendPasswordResetAsync(
        string email,
        Uri resetUrl,
        DateTimeOffset expiresAt,
        CancellationToken cancellationToken);

    /// <summary>Отправляет уведомление о событии безопасности.</summary>
    Task SendSecurityNotificationAsync(
        string email,
        string subject,
        string message,
        CancellationToken cancellationToken);
}

/// <summary>
/// Ошибка доставки транзакционного письма.
/// </summary>
public sealed class AuthEmailDeliveryException : Exception
{
    /// <summary>Создает исключение доставки письма.</summary>
    public AuthEmailDeliveryException()
    {
    }

    /// <summary>Создает исключение с описанием.</summary>
    public AuthEmailDeliveryException(string message)
        : base(message)
    {
    }

    /// <summary>Создает исключение с исходной инфраструктурной ошибкой.</summary>
    public AuthEmailDeliveryException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
