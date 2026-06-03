namespace MenuMate.Contracts.Auth;

/// <summary>
/// Запрос на регистрацию пользователя.
/// </summary>
/// <param name="Email">Email пользователя.</param>
/// <param name="DisplayName">Отображаемое имя пользователя.</param>
/// <param name="Password">Пароль в открытом виде.</param>
public sealed record RegisterUserRequest(string Email, string DisplayName, string Password);

/// <summary>
/// Запрос на выпуск токенов по email и паролю.
/// </summary>
/// <param name="Email">Email пользователя.</param>
/// <param name="Password">Пароль в открытом виде.</param>
public sealed record LoginUserRequest(string Email, string Password);

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
/// <param name="Roles">Названия назначенных ролей.</param>
public sealed record UserProfileResponse(Guid Id, string Email, string DisplayName, IReadOnlyCollection<string> Roles);

/// <summary>
/// Ответ после регистрации.
/// </summary>
/// <param name="User">Профиль зарегистрированного пользователя.</param>
/// <param name="Tokens">Выпущенная пара токенов.</param>
public sealed record RegisterUserResponse(UserProfileResponse User, TokenResponse Tokens);
