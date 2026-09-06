namespace MenuMate.Modules.Auth.Domain.Models;

/// <summary>
/// Состояние подтверждения адреса электронной почты пользователя.
/// </summary>
public enum EmailVerificationStatus
{
    /// <summary>Новая учетная запись ожидает подтверждения.</summary>
    PendingVerification,
    /// <summary>Учетная запись создана до внедрения подтверждения email.</summary>
    LegacyUnverified,
    /// <summary>Текущий адрес подтвержден.</summary>
    Verified
}
