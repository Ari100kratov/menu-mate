using MenuMate.Common.Application;
using MenuMate.Modules.Auth.Application;

namespace MenuMate.Modules.Auth.Application.RefreshUserToken;

internal sealed record RefreshUserTokenCommand(string RefreshToken) : ICommand<AuthSession>;
