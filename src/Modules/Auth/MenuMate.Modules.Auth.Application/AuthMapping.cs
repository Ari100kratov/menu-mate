using MenuMate.Contracts.Auth;
using MenuMate.Modules.Auth.Application.Abstractions;
using MenuMate.Modules.Auth.Domain.Models;

namespace MenuMate.Modules.Auth.Application;

internal static class AuthMapping
{
    public static UserProfileResponse ToProfile(User user) =>
        new(
            user.Id,
            user.Email,
            user.DisplayName,
            user.Roles.Select(role => role.RoleName).Order(StringComparer.Ordinal).ToArray(),
            new UserPreferencesResponse(user.ShowShoppingListPreview));

    public static TokenResponse ToTokenResponse(AccessToken accessToken) =>
        new(accessToken.Value, accessToken.ExpiresAt);
}
