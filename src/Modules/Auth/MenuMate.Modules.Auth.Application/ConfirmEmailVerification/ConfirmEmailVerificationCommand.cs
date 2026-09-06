using MenuMate.Common.Application;
using MenuMate.Contracts.Auth;

namespace MenuMate.Modules.Auth.Application.ConfirmEmailVerification;

internal sealed record ConfirmEmailVerificationCommand(ConfirmEmailVerificationRequest Request) : ICommand;
