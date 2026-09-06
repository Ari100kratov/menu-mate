using MenuMate.SharedKernel;

namespace MenuMate.Modules.Auth.Domain.Models;

/// <summary>
/// Ограниченное по времени одноразовое действие с учетной записью.
/// </summary>
public sealed class AccountAction : Entity<Guid>
{
    /// <summary>Максимальное количество неверных проверок.</summary>
    public const int MaximumFailedAttempts = 5;

    private AccountAction(
        Guid id,
        Guid userId,
        AccountActionPurpose purpose,
        string? targetEmail,
        string secretHash,
        DateTimeOffset createdAt,
        DateTimeOffset expiresAt,
        DateTimeOffset resendAvailableAt,
        DateTimeOffset? usedAt,
        int failedAttempts)
        : base(id)
    {
        UserId = userId;
        Purpose = purpose;
        TargetEmail = targetEmail;
        SecretHash = secretHash;
        CreatedAt = createdAt;
        ExpiresAt = expiresAt;
        ResendAvailableAt = resendAvailableAt;
        UsedAt = usedAt;
        FailedAttempts = failedAttempts;
    }

    /// <summary>Идентификатор владельца.</summary>
    public Guid UserId { get; }

    /// <summary>Назначение действия.</summary>
    public AccountActionPurpose Purpose { get; }

    /// <summary>Новый или подтверждаемый email, если он нужен сценарию.</summary>
    public string? TargetEmail { get; }

    /// <summary>HMAC-хеш одноразового секрета.</summary>
    public string SecretHash { get; }

    /// <summary>Момент создания.</summary>
    public DateTimeOffset CreatedAt { get; }

    /// <summary>Срок действия.</summary>
    public DateTimeOffset ExpiresAt { get; }

    /// <summary>Момент доступности повторной отправки.</summary>
    public DateTimeOffset ResendAvailableAt { get; }

    /// <summary>Момент использования или отзыва.</summary>
    public DateTimeOffset? UsedAt { get; private set; }

    /// <summary>Количество неуспешных проверок.</summary>
    public int FailedAttempts { get; private set; }

    /// <summary>Проверяет, можно ли еще использовать действие.</summary>
    public bool CanAttempt(DateTimeOffset now) =>
        UsedAt is null && ExpiresAt > now && FailedAttempts < MaximumFailedAttempts;

    /// <summary>Создает новое одноразовое действие.</summary>
    public static AccountAction Create(
        Guid id,
        Guid userId,
        AccountActionPurpose purpose,
        string? targetEmail,
        string secretHash,
        DateTimeOffset createdAt,
        DateTimeOffset expiresAt,
        DateTimeOffset resendAvailableAt) =>
        new(
            id,
            userId,
            purpose,
            targetEmail,
            secretHash,
            createdAt,
            expiresAt,
            resendAvailableAt,
            usedAt: null,
            failedAttempts: 0);

    /// <summary>Восстанавливает действие из persistence-снимка.</summary>
    public static AccountAction Rehydrate(
        Guid id,
        Guid userId,
        AccountActionPurpose purpose,
        string? targetEmail,
        string secretHash,
        DateTimeOffset createdAt,
        DateTimeOffset expiresAt,
        DateTimeOffset resendAvailableAt,
        DateTimeOffset? usedAt,
        int failedAttempts) =>
        new(
            id,
            userId,
            purpose,
            targetEmail,
            secretHash,
            createdAt,
            expiresAt,
            resendAvailableAt,
            usedAt,
            failedAttempts);

    /// <summary>Регистрирует неверную проверку секрета.</summary>
    public void RegisterFailedAttempt()
    {
        if (FailedAttempts < MaximumFailedAttempts)
        {
            FailedAttempts++;
        }
    }

    /// <summary>Помечает действие использованным.</summary>
    public void MarkUsed(DateTimeOffset now)
    {
        if (UsedAt is null)
        {
            UsedAt = now;
        }
    }
}
