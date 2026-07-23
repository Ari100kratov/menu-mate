using System.Security.Claims;
using System.Text;
using MenuMate.Modules.Auth.Application.Abstractions;
using MenuMate.Modules.Auth.Domain.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace MenuMate.Modules.Auth.Infrastructure.Authentication;

internal sealed class TokenProvider(IConfiguration configuration, TimeProvider timeProvider) : ITokenProvider
{
    public AccessToken Create(User user)
    {
        string secretKey = configuration["Jwt:Secret"] ?? AuthDefaults.JwtSecret;
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        DateTimeOffset expiresAt = timeProvider.GetUtcNow().AddMinutes(
            configuration.GetValue("Jwt:ExpirationInMinutes", AuthDefaults.AccessTokenExpirationInMinutes));

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.Name, user.DisplayName)
        };
        claims.AddRange(user.Roles.Select(role => new Claim(ClaimTypes.Role, role.RoleName)));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiresAt.UtcDateTime,
            SigningCredentials = credentials,
            Issuer = configuration["Jwt:Issuer"] ?? AuthDefaults.JwtIssuer,
            Audience = configuration["Jwt:Audience"] ?? AuthDefaults.JwtAudience
        };

        var handler = new JsonWebTokenHandler();

        return new AccessToken(handler.CreateToken(tokenDescriptor), expiresAt);
    }
}
