using MenuMate.Common.Application;
using MenuMate.Contracts.Auth;

namespace MenuMate.Modules.Auth.Application.RegisterUser;

internal sealed record RegisterUserCommand(RegisterUserRequest Request) : ICommand<RegisterUserResponse>;
