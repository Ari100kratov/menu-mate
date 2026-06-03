using MenuMate.SharedKernel;

namespace MenuMate.Modules.Auth.Domain.Models;

/// <summary>
/// Роль, назначаемая пользователям.
/// </summary>
public sealed class Role : Entity<Guid>
{
    private Role(Guid id, string name)
        : base(id)
    {
        Name = name;
    }

    /// <summary>
    /// Название роли.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Создает роль.
    /// </summary>
    public static Role Create(Guid id, string name) => new(id, name);
}
