using MenuMate.Common.Application;
using MenuMate.Modules.Auth.Application.Abstractions;
using MenuMate.Modules.Auth.Domain.Errors;
using MenuMate.Modules.Auth.Domain.Models;
using MenuMate.SharedKernel;
using Microsoft.Extensions.Logging;

namespace MenuMate.Modules.Auth.Application.ConfirmEmailChange;

internal sealed class ConfirmEmailChangeCommandHandler(
    IAuthRepository repository,
    IAuthUnitOfWork unitOfWork,
    IUserContext userContext,
    IAccountActionTokenService actionTokenService,
    IAuthEmailSender emailSender,
    TimeProvider timeProvider,
    ILogger<ConfirmEmailChangeCommandHandler> logger)
    : ICommandHandler<ConfirmEmailChangeCommand>
{
    public async Task<Result> Handle(
        ConfirmEmailChangeCommand command,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Request.Code) || command.Request.Code.Length != 6)
        {
            return Result.Failure(AuthErrors.InvalidOrExpiredAccountAction);
        }

        User? user = await repository.GetUserByIdAsync(userContext.UserId, cancellationToken);
        AccountAction? action = user is null
            ? null
            : await repository.GetLatestAccountActionAsync(
                user.Id,
                AccountActionPurpose.EmailChange,
                cancellationToken);
        DateTimeOffset now = timeProvider.GetUtcNow();

        if (user is null || action is null || action.TargetEmail is null || !action.CanAttempt(now))
        {
            return Result.Failure(AuthErrors.InvalidOrExpiredAccountAction);
        }

        if (!actionTokenService.Verify(command.Request.Code, action.SecretHash))
        {
            action.RegisterFailedAttempt();
            await repository.UpdateAccountActionAsync(action, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Failure(AuthErrors.InvalidOrExpiredAccountAction);
        }

        if (await repository.EmailExistsAsync(action.TargetEmail, cancellationToken))
        {
            return Result.Failure(AuthErrors.EmailNotUnique);
        }

        string oldEmail = user.Email;
        user.ChangeEmail(action.TargetEmail, now);
        action.MarkUsed(now);
        await repository.UpdateUserAsync(user, cancellationToken);
        await repository.UpdateAccountActionAsync(action, cancellationToken);
        await repository.RevokeRefreshTokensForUserAsync(userContext.UserId, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        try
        {
            await emailSender.SendSecurityNotificationAsync(
                oldEmail,
                "Email учетной записи MenuMate изменен",
                $"Email учетной записи изменен на {user.Email}. Если это были не вы, обратитесь в поддержку.",
                cancellationToken);
        }
        catch (AuthEmailDeliveryException exception)
        {
            AuthApplicationLogMessages.EmailDeliveryFailed(logger, "old email notification", exception);
        }

        return Result.Success();
    }
}
