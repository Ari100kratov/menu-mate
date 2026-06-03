using MenuMate.SharedKernel;
using MenuMate.SharedKernel.Identifiers;

namespace MenuMate.Modules.Auth.Domain.Errors;

/// <summary>
/// Ошибки домена и сценариев Auth.
/// </summary>
public static class AuthErrors
{
    /// <summary>
    /// Ошибка пустого email.
    /// </summary>
    public static readonly AppError EmptyEmail = AppError.Validation(
        "Auth.EmptyEmail",
        "Email обязателен.");

    /// <summary>
    /// Ошибка пустого отображаемого имени.
    /// </summary>
    public static readonly AppError EmptyDisplayName = AppError.Validation(
        "Auth.EmptyDisplayName",
        "Отображаемое имя обязательно.");

    /// <summary>
    /// Ошибка пустого хеша пароля.
    /// </summary>
    public static readonly AppError EmptyPasswordHash = AppError.Validation(
        "Auth.EmptyPasswordHash",
        "Хеш пароля обязателен.");

    /// <summary>
    /// Ошибка пустого пароля.
    /// </summary>
    public static readonly AppError EmptyPassword = AppError.Validation(
        "Auth.EmptyPassword",
        "Пароль обязателен.");

    /// <summary>
    /// Ошибка уникальности email.
    /// </summary>
    public static readonly AppError EmailNotUnique = AppError.Conflict(
        "Auth.EmailNotUnique",
        "Email уже используется.");

    /// <summary>
    /// Ошибка неверных учетных данных.
    /// </summary>
    public static readonly AppError InvalidCredentials = AppError.Validation(
        "Auth.InvalidCredentials",
        "Неверный email или пароль.");

    /// <summary>
    /// Ошибка неверного refresh token.
    /// </summary>
    public static readonly AppError InvalidRefreshToken = AppError.Validation(
        "Auth.InvalidRefreshToken",
        "Refresh token недействителен.");

    /// <summary>
    /// Ошибка отсутствующего пользователя.
    /// </summary>
    public static AppError UserNotFound(UserId userId) => AppError.NotFound(
        "Auth.UserNotFound",
        $"Пользователь с id '{userId}' не найден.");
}
