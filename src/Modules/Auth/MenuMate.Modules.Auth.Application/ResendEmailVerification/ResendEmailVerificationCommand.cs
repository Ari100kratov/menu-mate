using MenuMate.Common.Application;
using MenuMate.Contracts.Auth;

namespace MenuMate.Modules.Auth.Application.ResendEmailVerification;

internal sealed record ResendEmailVerificationCommand(ResendEmailVerificationRequest Request) : ICommand;
