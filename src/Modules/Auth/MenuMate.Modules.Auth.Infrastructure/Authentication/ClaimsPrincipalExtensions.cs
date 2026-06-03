using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;

namespace MenuMate.Modules.Auth.Infrastructure.Authentication;

internal static class ClaimsPrincipalExtensions
{
    public static Guid? GetUserId(this ClaimsPrincipal principal)
    {
        string? userId = principal.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);

        return Guid.TryParse(userId, out Guid value) ? value : null;
    }
}
