using MenuMate.Common.Application;
using MenuMate.Contracts.Auth;

namespace MenuMate.Modules.Auth.Application.RequestEmailChange;

internal sealed record RequestEmailChangeCommand(RequestEmailChangeRequest Request) : ICommand;
