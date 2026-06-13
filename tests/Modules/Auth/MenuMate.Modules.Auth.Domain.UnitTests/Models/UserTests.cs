using MenuMate.Modules.Auth.Domain.Errors;
using MenuMate.Modules.Auth.Domain.Models;
using MenuMate.SharedKernel;

namespace MenuMate.Modules.Auth.Domain.UnitTests.Models;

public sealed class UserTests
{
    private static readonly DateTimeOffset FixedNow = new(2026, 6, 12, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void CreateShouldNormalizeEmailAndTrimDisplayName()
    {
        User user = User.Create(
            Guid.CreateVersion7(),
            "  USER@Example.COM ",
            "  Пользователь  ",
            "hash",
            FixedNow).Value;

        Assert.Equal("user@example.com", user.Email);
        Assert.Equal("Пользователь", user.DisplayName);
        Assert.Equal(FixedNow, user.CreatedAt);
        Assert.Equal(FixedNow, user.UpdatedAt);
    }

    [Theory]
    [InlineData("", "Name", "hash", "email")]
    [InlineData("user@example.com", "", "hash", "displayName")]
    [InlineData("user@example.com", "Name", "", "passwordHash")]
    public void CreateShouldRejectRequiredValues(
        string email,
        string displayName,
        string passwordHash,
        string invalidField)
    {
        Result<User> result = User.Create(Guid.CreateVersion7(), email, displayName, passwordHash, FixedNow);

        Assert.True(result.IsFailure);
        Assert.Equal(
            invalidField switch
            {
                "email" => AuthErrors.EmptyEmail,
                "displayName" => AuthErrors.EmptyDisplayName,
                _ => AuthErrors.EmptyPasswordHash
            },
            result.Error);
    }

    [Fact]
    public void AddRoleShouldNotAddSameRoleTwice()
    {
        User user = CreateUser();
        var roleId = Guid.CreateVersion7();

        user.AddRole(roleId, "user");
        user.AddRole(roleId, "admin");

        Assert.Collection(
            user.Roles,
            role =>
            {
                Assert.Equal(roleId, role.RoleId);
                Assert.Equal("user", role.RoleName);
            });
    }

    [Fact]
    public void AddRefreshTokenShouldKeepIssuedToken()
    {
        User user = CreateUser();
        var token = RefreshToken.Create(
            Guid.CreateVersion7(),
            user.Id,
            "opaque-token",
            FixedNow.AddDays(7));

        user.AddRefreshToken(token);

        Assert.Same(token, Assert.Single(user.RefreshTokens));
    }

    private static User CreateUser() =>
        User.Create(Guid.CreateVersion7(), "user@example.com", "User", "hash", FixedNow).Value;
}
