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
public sealed record AdminUserListItemResponse(
    Guid Id,
    string Email,
    string DisplayName,
    DateTimeOffset RegisteredAt,
    IReadOnlyCollection<string> Roles,
    int RecipesCount);

/// <summary>
/// Ответ после регистрации.
/// </summary>
/// <param name="User">Профиль зарегистрированного пользователя.</param>
/// <param name="Tokens">Выпущенная пара токенов.</param>
public sealed record RegisterUserResponse(UserProfileResponse User, TokenResponse Tokens);
