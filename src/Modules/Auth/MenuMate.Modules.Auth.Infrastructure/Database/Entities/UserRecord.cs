using MenuMate.Modules.Auth.Domain.Models;

namespace MenuMate.Modules.Auth.Infrastructure.Database.Entities;

internal sealed class UserRecord
{
    public Guid Id { get; set; }

    public string Email { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public bool ShowShoppingListPreview { get; set; } = true;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public List<UserRoleRecord> Roles { get; set; } = [];

    public List<RefreshTokenRecord> RefreshTokens { get; set; } = [];

    public static UserRecord FromDomain(User user) =>
        new()
        {
            Id = user.Id,
            Email = user.Email,
            DisplayName = user.DisplayName,
            PasswordHash = user.PasswordHash,
            ShowShoppingListPreview = user.ShowShoppingListPreview,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            Roles = [.. user.Roles.Select(UserRoleRecord.FromDomain)],
            RefreshTokens = [.. user.RefreshTokens.Select(RefreshTokenRecord.FromDomain)]
        };

    public User ToDomain() =>
        User.Rehydrate(
            Id,
            Email,
            DisplayName,
            PasswordHash,
            ShowShoppingListPreview,
            CreatedAt,
            UpdatedAt,
            Roles.Select(role => role.ToDomain()),
            RefreshTokens.Select(token => token.ToDomain()));

    public void Apply(User user)
    {
        ShowShoppingListPreview = user.ShowShoppingListPreview;
        UpdatedAt = user.UpdatedAt;
    }
}
