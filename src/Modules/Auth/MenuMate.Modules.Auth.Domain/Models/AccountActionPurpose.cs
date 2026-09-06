namespace MenuMate.Modules.Auth.Domain.Models;

/// <summary>
/// Назначение одноразового действия с учетной записью.
/// </summary>
public enum AccountActionPurpose
{
    /// <summary>Первичное подтверждение регистрации.</summary>
    RegistrationEmailVerification,
    /// <summary>Подтверждение адреса прежней учетной записи.</summary>
    CurrentEmailVerification,
    /// <summary>Подтверждение нового адреса.</summary>
    EmailChange,
    /// <summary>Сброс забытого пароля.</summary>
    PasswordReset
}
