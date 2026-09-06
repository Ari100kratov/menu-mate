using MenuMate.Common.Application;
using MenuMate.Modules.Auth.Application.Abstractions;
using MenuMate.Modules.Auth.Domain.Errors;
using MenuMate.Modules.Auth.Domain.Models;
using MenuMate.SharedKernel;
using Microsoft.Extensions.Logging;

namespace MenuMate.Modules.Auth.Application.DeleteAccount;

internal sealed class DeleteAccountCommandHandler(
    IAuthRepository repository,
    IAuthUnitOfWork unitOfWork,
    IUserContext userContext,
    IPasswordHasher passwordHasher,
    IEnumerable<IUserDataEraser> dataErasers,
    IAuthEmailSender emailSender,
    ILogger<DeleteAccountCommandHandler> logger)
    : ICommandHandler<DeleteAccountCommand>
{
    public async Task<Result> Handle(DeleteAccountCommand command, CancellationToken cancellationToken)
    {
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

        foreach (IUserDataEraser eraser in dataErasers.OrderBy(eraser => eraser.Order))
        {
            await eraser.EraseAsync(userContext.UserId, cancellationToken);
        }

        string email = user.Email;
        await repository.DeleteUserAsync(userContext.UserId, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        try
        {
            await emailSender.SendSecurityNotificationAsync(
                email,
                "Учетная запись MenuMate удалена",
                "Учетная запись и связанные с ней данные удалены. Если это были не вы, обратитесь в поддержку.",
                cancellationToken);
        }
        catch (AuthEmailDeliveryException exception)
        {
            AuthApplicationLogMessages.EmailDeliveryFailed(logger, "account deletion notification", exception);
        }

        return Result.Success();
    }
}
