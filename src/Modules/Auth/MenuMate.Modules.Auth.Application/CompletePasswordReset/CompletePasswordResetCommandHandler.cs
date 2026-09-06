using MenuMate.Common.Application;
using MenuMate.Modules.Auth.Application.Abstractions;
using MenuMate.Modules.Auth.Domain.Errors;
using MenuMate.Modules.Auth.Domain.Models;
using MenuMate.SharedKernel;
using MenuMate.SharedKernel.Identifiers;
using Microsoft.Extensions.Logging;

namespace MenuMate.Modules.Auth.Application.CompletePasswordReset;

internal sealed class CompletePasswordResetCommandHandler(
    IAuthRepository repository,
    IAuthUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher,
    IAccountActionTokenService actionTokenService,
    IAuthEmailSender emailSender,
    TimeProvider timeProvider,
    ILogger<CompletePasswordResetCommandHandler> logger)
    : ICommandHandler<CompletePasswordResetCommand>
{
    public async Task<Result> Handle(
        CompletePasswordResetCommand command,
        CancellationToken cancellationToken)
    {
        Result validation = AuthInputValidator.ValidatePassword(command.Request.NewPassword);
        if (validation.IsFailure)
        {
            return validation;
        }

        if (string.IsNullOrWhiteSpace(command.Request.Token))
        {
            return Result.Failure(AuthErrors.InvalidOrExpiredAccountAction);
        }

        string hash = actionTokenService.Hash(command.Request.Token);
        AccountAction? action = await repository.GetAccountActionBySecretHashAsync(
            AccountActionPurpose.PasswordReset,
            hash,
            cancellationToken);
        DateTimeOffset now = timeProvider.GetUtcNow();
        if (action is null || !action.CanAttempt(now) ||
            !actionTokenService.Verify(command.Request.Token, action.SecretHash))
        {
            return Result.Failure(AuthErrors.InvalidOrExpiredAccountAction);
        }

        var userId = UserId.From(action.UserId);
        User? user = await repository.GetUserByIdAsync(userId, cancellationToken);
        if (user is null || user.EmailVerificationStatus != EmailVerificationStatus.Verified)
        {
            return Result.Failure(AuthErrors.InvalidOrExpiredAccountAction);
        }

        user.ChangePassword(passwordHasher.Hash(command.Request.NewPassword), now);
        action.MarkUsed(now);
        await repository.UpdateUserAsync(user, cancellationToken);
        await repository.UpdateAccountActionAsync(action, cancellationToken);
        await repository.RevokeRefreshTokensForUserAsync(userId, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        try
        {
            await emailSender.SendSecurityNotificationAsync(
                user.Email,
                "Пароль MenuMate сброшен",
                "Для учетной записи задан новый пароль. Если это были не вы, немедленно обратитесь в поддержку.",
                cancellationToken);
        }
        catch (AuthEmailDeliveryException exception)
        {
            AuthApplicationLogMessages.EmailDeliveryFailed(logger, "password reset notification", exception);
        }

        return Result.Success();
    }
}
