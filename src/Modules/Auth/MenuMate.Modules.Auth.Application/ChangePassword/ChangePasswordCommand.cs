using MenuMate.Common.Application;
using MenuMate.Contracts.Auth;

namespace MenuMate.Modules.Auth.Application.ChangePassword;

internal sealed record ChangePasswordCommand(ChangePasswordRequest Request) : ICommand;
