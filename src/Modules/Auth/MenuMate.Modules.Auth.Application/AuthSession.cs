using MenuMate.Contracts.Auth;
using MenuMate.Modules.Auth.Domain.Models;

namespace MenuMate.Modules.Auth.Application;

internal sealed record AuthSession(TokenResponse Tokens, RefreshToken RefreshToken);

internal sealed record RegisterUserSession(
    UserProfileResponse User,
    TokenResponse Tokens,
    RefreshToken RefreshToken);
