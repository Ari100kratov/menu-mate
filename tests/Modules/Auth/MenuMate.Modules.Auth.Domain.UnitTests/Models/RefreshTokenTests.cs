using MenuMate.Modules.Auth.Domain.Models;

namespace MenuMate.Modules.Auth.Domain.UnitTests.Models;

public sealed class RefreshTokenTests
{
    [Fact]
    public void RevokeShouldMarkTokenAsRevoked()
    {
        var token = RefreshToken.Create(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            "opaque-token",
            DateTimeOffset.UtcNow.AddDays(1));

        token.Revoke();

        Assert.True(token.IsRevoked);
    }

    [Fact]
    public void RehydrateShouldRestoreRevokedState()
    {
        var token = RefreshToken.Rehydrate(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            "opaque-token",
            DateTimeOffset.UtcNow.AddDays(1),
            isRevoked: true);

        Assert.True(token.IsRevoked);
    }
}
