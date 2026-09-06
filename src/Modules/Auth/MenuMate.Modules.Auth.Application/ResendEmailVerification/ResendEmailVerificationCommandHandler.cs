using MenuMate.Common.Application;
using MenuMate.Modules.Auth.Application.Abstractions;
using MenuMate.Modules.Auth.Domain.Models;
using MenuMate.Modules.Auth.Domain.ValueObjects;
using MenuMate.SharedKernel;
using Microsoft.Extensions.Logging;

namespace MenuMate.Modules.Auth.Application.ResendEmailVerification;

internal sealed class ResendEmailVerificationCommandHandler(
    IAuthRepository repository,
    IAuthUnitOfWork unitOfWork,
    IAccountActionTokenService actionTokenService,
    IAuthEmailSender emailSender,
    TimeProvider timeProvider,
    ILogger<ResendEmailVerificationCommandHandler> logger)
    : ICommandHandler<ResendEmailVerificationCommand>
{
    public async Task<Result> Handle(
        ResendEmailVerificationCommand command,
        CancellationToken cancellationToken)
    {
        if (AuthInputValidator.ValidateEmail(command.Request.Email).IsFailure)
        {
            return Result.Success();
        }

        User? user = await repository.GetUserByEmailAsync(
            EmailNormalizer.Normalize(command.Request.Email),
            cancellationToken);
        if (user is null || user.EmailVerificationStatus == EmailVerificationStatus.Verified)
        {
            return Result.Success();
        }

        AccountActionPurpose purpose = user.EmailVerificationStatus == EmailVerificationStatus.PendingVerification
            ? AccountActionPurpose.RegistrationEmailVerification
            : AccountActionPurpose.CurrentEmailVerification;
        DateTimeOffset now = timeProvider.GetUtcNow();
        AccountAction? previous = await repository.GetLatestAccountActionAsync(user.Id, purpose, cancellationToken);
        if (previous is not null && previous.UsedAt is null && previous.ResendAvailableAt > now)
        {
            return Result.Success();
        }

        await repository.InvalidateAccountActionsAsync(user.Id, purpose, now, cancellationToken);
        string code = actionTokenService.GenerateCode();
        DateTimeOffset expiresAt = now.Add(AccountActionPolicy.VerificationCodeLifetime);
        var action = AccountAction.Create(
            Guid.CreateVersion7(),
            user.Id,
            purpose,
            user.Email,
            actionTokenService.Hash(code),
            now,
            expiresAt,
            now.Add(AccountActionPolicy.ResendCooldown));
        await repository.AddAccountActionAsync(action, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        try
        {
            await emailSender.SendVerificationCodeAsync(user.Email, code, expiresAt, cancellationToken);
        }
        catch (AuthEmailDeliveryException exception)
        {
            AuthApplicationLogMessages.EmailDeliveryFailed(logger, "email verification resend", exception);
        }

        return Result.Success();
    }
}
