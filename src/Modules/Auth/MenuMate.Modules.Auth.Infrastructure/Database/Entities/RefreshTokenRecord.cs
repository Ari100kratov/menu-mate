using MenuMate.Modules.Auth.Domain.Models;

namespace MenuMate.Modules.Auth.Infrastructure.Database.Entities;

internal sealed class RefreshTokenRecord
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string Value { get; set; } = string.Empty;

    public DateTimeOffset ExpiresAt { get; set; }

    public bool IsRevoked { get; set; }

    public static RefreshTokenRecord FromDomain(RefreshToken refreshToken) =>
        new()
        {
            Id = refreshToken.Id,
            UserId = refreshToken.UserId,
            Value = refreshToken.Value,
            ExpiresAt = refreshToken.ExpiresAt,
            IsRevoked = refreshToken.IsRevoked
        };

    public RefreshToken ToDomain() =>
        RefreshToken.Rehydrate(Id, UserId, Value, ExpiresAt, IsRevoked);
}
