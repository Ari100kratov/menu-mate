namespace MenuMate.Contracts.Auth;

/// <summary>
/// Запрос на регистрацию пользователя.
/// </summary>
/// <param name="Email">Email пользователя.</param>
/// <param name="DisplayName">Отображаемое имя пользователя.</param>
/// <param name="Password">Пароль в открытом виде.</param>
/// <param name="PrivacyPolicyVersion">Версия принятой политики конфиденциальности.</param>
public sealed record RegisterUserRequest(
    string Email,
    string DisplayName,
    string Password,
    string PrivacyPolicyVersion);

/// <summary>
/// Запрос на выпуск токенов по email и паролю.
/// </summary>
/// <param name="Email">Email пользователя.</param>
/// <param name="Password">Пароль в открытом виде.</param>
public sealed record LoginUserRequest(string Email, string Password);

/// <summary>
/// Запрос на подтверждение текущего адреса электронной почты.
/// </summary>
public sealed record ConfirmEmailVerificationRequest(string Email, string Code);

/// <summary>
/// Запрос на повторную отправку кода подтверждения.
/// </summary>
public sealed record ResendEmailVerificationRequest(string Email);

/// <summary>
/// Запрос ссылки для сброса пароля.
/// </summary>
public sealed record RequestPasswordResetRequest(string Email);

/// <summary>
/// Завершение сброса пароля.
/// </summary>
public sealed record CompletePasswordResetRequest(string Token, string NewPassword);

/// <summary>
/// Изменение отображаемого имени.
/// </summary>
public sealed record UpdateDisplayNameRequest(string DisplayName);

/// <summary>
/// Запрос изменения email с подтверждением текущего пароля.
/// </summary>
public sealed record RequestEmailChangeRequest(string NewEmail, string CurrentPassword);

/// <summary>
/// Подтверждение нового email одноразовым кодом.
/// </summary>
public sealed record ConfirmEmailChangeRequest(string Code);

/// <summary>
/// Изменение пароля авторизованного пользователя.
/// </summary>
public sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword);

/// <summary>Подтверждение актуальной политики конфиденциальности.</summary>
public sealed record AcceptPrivacyPolicyRequest(string PrivacyPolicyVersion);

/// <summary>Необратимое удаление учетной записи после повторной проверки пароля.</summary>
public sealed record DeleteAccountRequest(string CurrentPassword);

/// <summary>Публичные реквизиты актуальной политики конфиденциальности.</summary>
public sealed record PrivacyPolicyResponse(
    string Version,
    DateTimeOffset EffectiveAt,
    string OperatorName,
    string ContactEmail,
    int TechnicalLogRetentionDays,
    int BackupRetentionDays);

/// <summary>
/// Access token, возвращаемый auth endpoints.
/// </summary>
/// <param name="AccessToken">JWT-токен доступа.</param>
/// <param name="ExpiresAt">Момент истечения токена доступа.</param>
public sealed record TokenResponse(string AccessToken, DateTimeOffset ExpiresAt);

/// <summary>
/// Публичный профиль пользователя.
/// </summary>
/// <param name="Id">Идентификатор пользователя.</param>
/// <param name="Email">Email пользователя.</param>
/// <param name="DisplayName">Отображаемое имя пользователя.</param>
/// <param name="EmailVerificationStatus">Состояние подтверждения email.</param>
/// <param name="PrivacyPolicyAcceptedVersion">Принятая пользователем версия политики.</param>
/// <param name="RequiresPrivacyPolicyAcceptance">Требуется ли принять актуальную версию.</param>
/// <param name="Roles">Названия назначенных ролей.</param>
/// <param name="Preferences">Пользовательские настройки приложения.</param>
public sealed record UserProfileResponse(
    Guid Id,
    string Email,
    string DisplayName,
    string EmailVerificationStatus,
    string? PrivacyPolicyAcceptedVersion,
    bool RequiresPrivacyPolicyAcceptance,
    IReadOnlyCollection<string> Roles,
    UserPreferencesResponse Preferences);

/// <summary>
/// Пользовательские настройки приложения.
/// </summary>
/// <param name="ShowShoppingListPreview">Показывать ли предпросмотр перед созданием списка покупок из меню.</param>
public sealed record UserPreferencesResponse(bool ShowShoppingListPreview);

/// <summary>
/// Запрос на изменение пользовательских настроек приложения.
/// </summary>
/// <param name="ShowShoppingListPreview">Показывать ли предпросмотр перед созданием списка покупок из меню.</param>
public sealed record UpdateUserPreferencesRequest(bool ShowShoppingListPreview);

/// <summary>
/// Страница пользователей для административного просмотра.
/// </summary>
/// <param name="Items">Пользователи на текущей странице.</param>
/// <param name="TotalCount">Общее количество пользователей, соответствующих фильтру.</param>
/// <param name="Page">Номер текущей страницы.</param>
/// <param name="PageSize">Количество пользователей на странице.</param>
public sealed record AdminUsersPageResponse(
    IReadOnlyCollection<AdminUserListItemResponse> Items,
    int TotalCount,
    int Page,
    int PageSize);

/// <summary>
/// Краткая информация о зарегистрированном пользователе для администратора.
/// </summary>
/// <param name="Id">Идентификатор пользователя.</param>
/// <param name="Email">Email пользователя.</param>
/// <param name="DisplayName">Отображаемое имя пользователя.</param>
/// <param name="RegisteredAt">Дата и время регистрации.</param>
/// <param name="Roles">Назначенные роли.</param>
/// <param name="RecipesCount">Количество активных рецептов пользователя.</param>
/// <param name="FavoriteCount">Количество рецептов, добавленных пользователем в избранное.</param>
public sealed record AdminUserListItemResponse(
    Guid Id,
    string Email,
    string DisplayName,
    DateTimeOffset RegisteredAt,
    IReadOnlyCollection<string> Roles,
    int RecipesCount,
    int FavoriteCount);

/// <summary>
/// Ответ после регистрации.
/// </summary>
/// <param name="Email">Адрес, на который отправлен код.</param>
/// <param name="CodeExpiresAt">Срок действия кода.</param>
/// <param name="ResendAvailableAt">Момент, после которого код можно отправить повторно.</param>
public sealed record RegisterUserResponse(
    string Email,
    DateTimeOffset CodeExpiresAt,
    DateTimeOffset ResendAvailableAt);
