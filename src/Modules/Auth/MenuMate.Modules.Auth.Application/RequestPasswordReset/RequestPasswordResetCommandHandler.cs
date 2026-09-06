using MenuMate.Common.Application;
using MenuMate.Modules.Auth.Application.Abstractions;
using MenuMate.Modules.Auth.Domain.Models;
using MenuMate.Modules.Auth.Domain.ValueObjects;
using MenuMate.SharedKernel;
using Microsoft.Extensions.Logging;

namespace MenuMate.Modules.Auth.Application.RequestPasswordReset;

internal sealed class RequestPasswordResetCommandHandler(
    IAuthRepository repository,
    IAuthUnitOfWork unitOfWork,
    IAccountActionTokenService actionTokenService,
    IAuthEmailSender emailSender,
    IAuthUrlBuilder urlBuilder,
    TimeProvider timeProvider,
    ILogger<RequestPasswordResetCommandHandler> logger)
    : ICommandHandler<RequestPasswordResetCommand>
{
    public async Task<Result> Handle(
        RequestPasswordResetCommand command,
        CancellationToken cancellationToken)
    {
        if (AuthInputValidator.ValidateEmail(command.Request.Email).IsFailure)
        {
            return Result.Success();
        }

        User? user = await repository.GetUserByEmailAsync(
            EmailNormalizer.Normalize(command.Request.Email),
            cancellationToken);
        if (user is null || user.EmailVerificationStatus != EmailVerificationStatus.Verified)
        {
            return Result.Success();
        }

        DateTimeOffset now = timeProvider.GetUtcNow();
        AccountAction? previous = await repository.GetLatestAccountActionAsync(
            user.Id,
            AccountActionPurpose.PasswordReset,
            cancellationToken);
        if (previous is not null && previous.UsedAt is null && previous.ResendAvailableAt > now)
        {
            return Result.Success();
        }

        await repository.InvalidateAccountActionsAsync(
            user.Id,
            AccountActionPurpose.PasswordReset,
            now,
            cancellationToken);
        string token = actionTokenService.GenerateToken();
        DateTimeOffset expiresAt = now.Add(AccountActionPolicy.PasswordResetLifetime);
        var action = AccountAction.Create(
            Guid.CreateVersion7(),
            user.Id,
            AccountActionPurpose.PasswordReset,
            user.Email,
            actionTokenService.Hash(token),
            now,
            expiresAt,
            now.Add(AccountActionPolicy.ResendCooldown));
        await repository.AddAccountActionAsync(action, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        try
        {
            await emailSender.SendPasswordResetAsync(
                user.Email,
                urlBuilder.BuildPasswordResetUrl(token),
                expiresAt,
                cancellationToken);
        }
        catch (AuthEmailDeliveryException exception)
        {
            AuthApplicationLogMessages.EmailDeliveryFailed(logger, "password reset request", exception);
        }

        return Result.Success();
    }
}
