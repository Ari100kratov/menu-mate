using MenuMate.Common.Application;
using MenuMate.Contracts.Auth;

namespace MenuMate.Modules.Auth.Application.DeleteAccount;

internal sealed record DeleteAccountCommand(DeleteAccountRequest Request) : ICommand;
