using MenuMate.Common.Application;
using MenuMate.Contracts.Auth;

namespace MenuMate.Modules.Auth.Application.ConfirmEmailChange;

internal sealed record ConfirmEmailChangeCommand(ConfirmEmailChangeRequest Request) : ICommand;
