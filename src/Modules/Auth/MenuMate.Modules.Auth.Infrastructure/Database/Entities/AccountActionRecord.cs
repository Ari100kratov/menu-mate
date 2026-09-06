using MenuMate.Modules.Auth.Domain.Models;

namespace MenuMate.Modules.Auth.Infrastructure.Database.Entities;

internal sealed class AccountActionRecord
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public AccountActionPurpose Purpose { get; set; }
    public string? TargetEmail { get; set; }
    public string SecretHash { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset ResendAvailableAt { get; set; }
    public DateTimeOffset? UsedAt { get; set; }
    public int FailedAttempts { get; set; }

    public static AccountActionRecord FromDomain(AccountAction action) => new()
    {
        Id = action.Id,
        UserId = action.UserId,
        Purpose = action.Purpose,
        TargetEmail = action.TargetEmail,
        SecretHash = action.SecretHash,
        CreatedAt = action.CreatedAt,
        ExpiresAt = action.ExpiresAt,
        ResendAvailableAt = action.ResendAvailableAt,
        UsedAt = action.UsedAt,
        FailedAttempts = action.FailedAttempts
    };

    public AccountAction ToDomain() => AccountAction.Rehydrate(
        Id,
        UserId,
        Purpose,
        TargetEmail,
        SecretHash,
        CreatedAt,
        ExpiresAt,
        ResendAvailableAt,
        UsedAt,
        FailedAttempts);

    public void Apply(AccountAction action)
    {
        UsedAt = action.UsedAt;
        FailedAttempts = action.FailedAttempts;
    }
}
