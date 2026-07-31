using MenuMate.Modules.Auth.Domain.Errors;
using MenuMate.Modules.Auth.Domain.ValueObjects;
using MenuMate.SharedKernel;

namespace MenuMate.Modules.Auth.Domain.Models;

/// <summary>
/// Учетная запись пользователя.
/// </summary>
public sealed class User : Entity<Guid>
{
    private readonly List<UserRole> _roles = [];
    private readonly List<RefreshToken> _refreshTokens = [];

    private User(
        Guid id,
        string email,
        string displayName,
        string passwordHash,
        bool showShoppingListPreview,
        DateTimeOffset createdAt)
        : base(id)
    {
        Email = email;
        DisplayName = displayName;
        PasswordHash = passwordHash;
        ShowShoppingListPreview = showShoppingListPreview;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    /// <summary>
    /// Email пользователя.
    /// </summary>
    public string Email { get; private set; }

    /// <summary>
    /// Отображаемое имя пользователя.
    /// </summary>
    public string DisplayName { get; private set; }

    /// <summary>
    /// Хеш пароля.
    /// </summary>
    public string PasswordHash { get; private set; }

    /// <summary>
    /// Показывать ли предпросмотр перед созданием списка покупок из меню.
    /// </summary>
    public bool ShowShoppingListPreview { get; private set; }

    /// <summary>
    /// Account creation moment.
    /// </summary>
    public DateTimeOffset CreatedAt { get; }

    /// <summary>
    /// Last update moment.
    /// </summary>
    public DateTimeOffset UpdatedAt { get; private set; }

    /// <summary>
    /// Assigned roles.
    /// </summary>
    public IReadOnlyCollection<UserRole> Roles => _roles.AsReadOnly();

    /// <summary>
    /// Выданные refresh-токены.
    /// </summary>
    public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

    /// <summary>
    /// Создает нового пользователя.
    /// </summary>
    public static Result<User> Create(
        Guid id,
        string email,
        string displayName,
        string passwordHash,
        DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return Result.Failure<User>(AuthErrors.EmptyEmail);
        }

        if (string.IsNullOrWhiteSpace(displayName))
        {
            return Result.Failure<User>(AuthErrors.EmptyDisplayName);
        }

        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            return Result.Failure<User>(AuthErrors.EmptyPasswordHash);
        }

        return new User(
            id,
            EmailNormalizer.Normalize(email),
            displayName.Trim(),
            passwordHash,
            showShoppingListPreview: true,
            now);
    }

    /// <summary>
    /// Восстанавливает пользователя из persistence-снимка.
    /// </summary>
    public static User Rehydrate(
        Guid id,
        string email,
        string displayName,
        string passwordHash,
        bool showShoppingListPreview,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt,
        IEnumerable<UserRole> roles,
        IEnumerable<RefreshToken> refreshTokens)
    {
        var user = new User(id, email, displayName, passwordHash, showShoppingListPreview, createdAt)
        {
            UpdatedAt = updatedAt
        };

        user._roles.AddRange(roles);
        user._refreshTokens.AddRange(refreshTokens);

        return user;
    }

    /// <summary>
    /// Назначает роль пользователю.
    /// </summary>
    public void AddRole(Guid roleId, string roleName = "")
    {
        if (_roles.Any(role => role.RoleId == roleId))
        {
            return;
        }

        _roles.Add(new UserRole(Id, roleId, roleName));
    }

    /// <summary>
    /// Добавляет refresh token пользователю.
    /// </summary>
    public void AddRefreshToken(RefreshToken refreshToken) => _refreshTokens.Add(refreshToken);

    /// <summary>
    /// Изменяет пользовательские настройки приложения.
    /// </summary>
    public void UpdatePreferences(bool showShoppingListPreview, DateTimeOffset now)
    {
        if (ShowShoppingListPreview == showShoppingListPreview)
        {
            return;
        }

        ShowShoppingListPreview = showShoppingListPreview;
        UpdatedAt = now;
    }
}
