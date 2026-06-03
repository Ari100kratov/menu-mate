using MenuMate.Modules.Auth.Domain.Models;

namespace MenuMate.Modules.Auth.Infrastructure.Database.Entities;

internal sealed class RoleRecord
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public List<UserRoleRecord> Users { get; set; } = [];

    public Role ToDomain() => Role.Create(Id, Name);
}
