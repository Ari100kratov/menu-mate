using MenuMate.Common.Application;
using MenuMate.Contracts.Auth;
using MenuMate.Modules.Auth.Application;

namespace MenuMate.Modules.Auth.Application.LoginUser;

internal sealed record LoginUserCommand(LoginUserRequest Request) : ICommand<AuthSession>;
