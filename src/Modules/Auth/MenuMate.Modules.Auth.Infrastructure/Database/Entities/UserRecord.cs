using MenuMate.Modules.Auth.Domain.Models;

namespace MenuMate.Modules.Auth.Infrastructure.Database.Entities;

internal sealed class UserRecord
{
    public Guid Id { get; set; }

    public string Email { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public EmailVerificationStatus EmailVerificationStatus { get; set; }

    public string? PrivacyPolicyAcceptedVersion { get; set; }

    public DateTimeOffset? PrivacyPolicyAcceptedAt { get; set; }

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
            EmailVerificationStatus = user.EmailVerificationStatus,
            PrivacyPolicyAcceptedVersion = user.PrivacyPolicyAcceptedVersion,
            PrivacyPolicyAcceptedAt = user.PrivacyPolicyAcceptedAt,
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
            EmailVerificationStatus,
            PrivacyPolicyAcceptedVersion,
            PrivacyPolicyAcceptedAt,
            ShowShoppingListPreview,
            CreatedAt,
            UpdatedAt,
            Roles.Select(role => role.ToDomain()),
            RefreshTokens.Select(token => token.ToDomain()));

    public void Apply(User user)
    {
        Email = user.Email;
        DisplayName = user.DisplayName;
        PasswordHash = user.PasswordHash;
        EmailVerificationStatus = user.EmailVerificationStatus;
        PrivacyPolicyAcceptedVersion = user.PrivacyPolicyAcceptedVersion;
        PrivacyPolicyAcceptedAt = user.PrivacyPolicyAcceptedAt;
        ShowShoppingListPreview = user.ShowShoppingListPreview;
        UpdatedAt = user.UpdatedAt;
    }
}
