using System.Security.Cryptography;
using MenuMate.Modules.Auth.Application.Abstractions;
using MenuMate.Modules.Auth.Domain.Models;

namespace MenuMate.Modules.Auth.Infrastructure.Authentication;

internal sealed class RefreshTokenService(TimeProvider timeProvider) : IRefreshTokenService
{
    public RefreshToken Generate(User user, int daysValid = 7)
    {
        byte[] bytes = RandomNumberGenerator.GetBytes(64);
        string value = Convert.ToBase64String(bytes);

        return RefreshToken.Create(
            Guid.CreateVersion7(),
            user.Id,
            value,
            timeProvider.GetUtcNow().AddDays(daysValid));
    }

    public bool IsValid(RefreshToken token) =>
        !token.IsRevoked && token.ExpiresAt > timeProvider.GetUtcNow();
}
