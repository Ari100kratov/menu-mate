using MenuMate.Common.Application;
using MenuMate.Contracts.Auth;
using MenuMate.Modules.Auth.Application;

namespace MenuMate.Modules.Auth.Application.RegisterUser;

internal sealed record RegisterUserCommand(RegisterUserRequest Request) : ICommand<RegisterUserSession>;
