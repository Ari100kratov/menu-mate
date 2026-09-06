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

    /// <summary>Ошибка длины пароля.</summary>
    public static readonly AppError InvalidPasswordLength = AppError.Validation(
        "Auth.InvalidPasswordLength",
        "Пароль должен содержать от 8 до 128 символов.");

    /// <summary>Ошибка длины отображаемого имени.</summary>
    public static readonly AppError InvalidDisplayName = AppError.Validation(
        "Auth.InvalidDisplayName",
        "Имя должно содержать от 1 до 120 символов.");

    /// <summary>Ошибка формата email.</summary>
    public static readonly AppError InvalidEmail = AppError.Validation(
        "Auth.InvalidEmail",
        "Укажите корректный email.");

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

    /// <summary>Вход до первичного подтверждения email запрещен.</summary>
    public static readonly AppError EmailNotVerified = AppError.Forbidden(
        "Auth.EmailNotVerified",
        "Подтвердите адрес электронной почты перед входом.");

    /// <summary>Одноразовое действие недействительно.</summary>
    public static readonly AppError InvalidOrExpiredAccountAction = AppError.Validation(
        "Auth.InvalidOrExpiredAccountAction",
        "Код или ссылка недействительны либо срок действия истек.");

    /// <summary>Повторная отправка временно недоступна.</summary>
    public static readonly AppError ResendNotAvailable = AppError.Conflict(
        "Auth.ResendNotAvailable",
        "Новый код пока нельзя отправить. Повторите попытку позже.");

    /// <summary>Ошибка доставки транзакционного письма.</summary>
    public static readonly AppError EmailDeliveryFailed = AppError.Problem(
        "Auth.EmailDeliveryFailed",
        "Не удалось отправить письмо. Повторите попытку позже.");

    /// <summary>Ошибка доставки письма после сохранения новой учетной записи.</summary>
    public static readonly AppError RegistrationEmailDeliveryFailed = AppError.Problem(
        "Auth.RegistrationEmailDeliveryFailed",
        "Учетная запись создана, но письмо не отправлено. Перейдите к подтверждению и запросите новый код.");

    /// <summary>Клиент должен показать и принять актуальную политику.</summary>
    public static readonly AppError PrivacyPolicyOutdated = AppError.Validation(
        "Auth.PrivacyPolicyOutdated",
        "Ознакомьтесь с актуальной политикой конфиденциальности и подтвердите ее.");

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
