using MenuMate.SharedKernel;

namespace MenuMate.Modules.Auth.Domain.Models;

/// <summary>
/// Opaque refresh token.
/// </summary>
public sealed class RefreshToken : Entity<Guid>
{
    private RefreshToken(Guid id, Guid userId, string value, DateTimeOffset expiresAt, bool isRevoked)
        : base(id)
    {
        UserId = userId;
        Value = value;
        ExpiresAt = expiresAt;
        IsRevoked = isRevoked;
    }

    /// <summary>
    /// Идентификатор пользователя.
    /// </summary>
    public Guid UserId { get; }

    /// <summary>
    /// Значение токена.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Expiration moment.
    /// </summary>
    public DateTimeOffset ExpiresAt { get; }

    /// <summary>
    /// Revocation flag.
    /// </summary>
    public bool IsRevoked { get; private set; }

    /// <summary>
    /// Создает refresh token.
    /// </summary>
    public static RefreshToken Create(Guid id, Guid userId, string value, DateTimeOffset expiresAt) =>
        new(id, userId, value, expiresAt, isRevoked: false);

    /// <summary>
    /// Rehydrates a refresh token from persistence.
    /// </summary>
    public static RefreshToken Rehydrate(
        Guid id,
        Guid userId,
        string value,
        DateTimeOffset expiresAt,
        bool isRevoked) =>
        new(id, userId, value, expiresAt, isRevoked);

    /// <summary>
    /// Отзывает refresh-токен.
    /// </summary>
    public void Revoke() => IsRevoked = true;
}
