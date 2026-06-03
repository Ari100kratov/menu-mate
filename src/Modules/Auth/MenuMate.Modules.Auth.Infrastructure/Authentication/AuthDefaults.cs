namespace MenuMate.Modules.Auth.Infrastructure.Authentication;

internal static class AuthDefaults
{
    public const string JwtIssuer = "MenuMate";

    public const string JwtAudience = "MenuMate";

    public const string JwtSecret = "local-development-secret-at-least-32-bytes";

    public const int AccessTokenExpirationInMinutes = 60;
}
