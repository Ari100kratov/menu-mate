using MenuMate.Common.Application;
using MenuMate.Contracts.Auth;

namespace MenuMate.Modules.Auth.Application.RequestPasswordReset;

internal sealed record RequestPasswordResetCommand(RequestPasswordResetRequest Request) : ICommand;
