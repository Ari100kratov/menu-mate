using MenuMate.Common.Application;
using MenuMate.Contracts.Auth;

namespace MenuMate.Modules.Auth.Application.CompletePasswordReset;

internal sealed record CompletePasswordResetCommand(CompletePasswordResetRequest Request) : ICommand;
