using MenuMate.Modules.Auth.Domain.ValueObjects;

namespace MenuMate.Modules.Auth.Infrastructure.Database.Entities;

internal sealed class UserRoleRecord
{
    public Guid UserId { get; set; }

    public Guid RoleId { get; set; }

    public RoleRecord? Role { get; set; }

    public static UserRoleRecord FromDomain(UserRole role) =>
        new()
        {
            UserId = role.UserId,
            RoleId = role.RoleId
        };

    public UserRole ToDomain() => new(UserId, RoleId, Role?.Name ?? string.Empty);
}
