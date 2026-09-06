using MenuMate.Common.Application;
using MenuMate.Modules.Auth.Application.Abstractions;
using MenuMate.Modules.Auth.Domain.Errors;
using MenuMate.Modules.Auth.Domain.Models;
using MenuMate.SharedKernel;
using Microsoft.Extensions.Logging;

namespace MenuMate.Modules.Auth.Application.ChangePassword;

internal sealed class ChangePasswordCommandHandler(
    IAuthRepository repository,
    IAuthUnitOfWork unitOfWork,
    IUserContext userContext,
    IPasswordHasher passwordHasher,
    IAuthEmailSender emailSender,
    TimeProvider timeProvider,
    ILogger<ChangePasswordCommandHandler> logger)
    : ICommandHandler<ChangePasswordCommand>
{
    public async Task<Result> Handle(ChangePasswordCommand command, CancellationToken cancellationToken)
    {
        Result validation = AuthInputValidator.ValidatePassword(command.Request.NewPassword);
        if (validation.IsFailure)
        {
            return validation;
        }

        User? user = await repository.GetUserByIdAsync(userContext.UserId, cancellationToken);
        if (user is null)
        {
            return Result.Failure(AuthErrors.UserNotFound(userContext.UserId));
        }

        if (string.IsNullOrEmpty(command.Request.CurrentPassword) ||
            !passwordHasher.Verify(command.Request.CurrentPassword, user.PasswordHash))
        {
            return Result.Failure(AuthErrors.InvalidCredentials);
        }

        user.ChangePassword(passwordHasher.Hash(command.Request.NewPassword), timeProvider.GetUtcNow());
        await repository.UpdateUserAsync(user, cancellationToken);
        await repository.RevokeRefreshTokensForUserAsync(userContext.UserId, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        if (user.EmailVerificationStatus == EmailVerificationStatus.Verified)
        {
            try
            {
                await emailSender.SendSecurityNotificationAsync(
                    user.Email,
                    "Пароль MenuMate изменен",
                    "Пароль учетной записи изменен. Если это были не вы, немедленно восстановите доступ.",
                    cancellationToken);
            }
            catch (AuthEmailDeliveryException exception)
            {
                AuthApplicationLogMessages.EmailDeliveryFailed(logger, "password change notification", exception);
            }
        }

        return Result.Success();
    }
}
