namespace MenuMate.Modules.Auth.Domain.ValueObjects;

/// <summary>
/// Назначение роли пользователю.
/// </summary>
public sealed class UserRole
{
    /// <summary>
    /// Создает назначение роли пользователю.
    /// </summary>
    public UserRole(Guid userId, Guid roleId, string roleName = "")
    {
        UserId = userId;
        RoleId = roleId;
        RoleName = roleName;
    }

    /// <summary>
    /// Идентификатор пользователя.
    /// </summary>
    public Guid UserId { get; }

    /// <summary>
    /// Идентификатор роли.
    /// </summary>
    public Guid RoleId { get; }

    /// <summary>
    /// Снимок названия роли.
    /// </summary>
    public string RoleName { get; }
}
