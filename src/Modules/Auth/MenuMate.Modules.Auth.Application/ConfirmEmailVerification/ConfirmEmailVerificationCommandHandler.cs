using MenuMate.Common.Application;
using MenuMate.Modules.Auth.Application.Abstractions;
using MenuMate.Modules.Auth.Domain.Errors;
using MenuMate.Modules.Auth.Domain.Models;
using MenuMate.Modules.Auth.Domain.ValueObjects;
using MenuMate.SharedKernel;
using MenuMate.SharedKernel.Identifiers;

namespace MenuMate.Modules.Auth.Application.ConfirmEmailVerification;

internal sealed class ConfirmEmailVerificationCommandHandler(
    IAuthRepository repository,
    IAuthUnitOfWork unitOfWork,
    IAccountActionTokenService actionTokenService,
    IEnumerable<IUserRegistrationInitializer> registrationInitializers,
    TimeProvider timeProvider)
    : ICommandHandler<ConfirmEmailVerificationCommand>
{
    public async Task<Result> Handle(
        ConfirmEmailVerificationCommand command,
        CancellationToken cancellationToken)
    {
        if (AuthInputValidator.ValidateEmail(command.Request.Email).IsFailure ||
            string.IsNullOrWhiteSpace(command.Request.Code) ||
            command.Request.Code.Length != 6)
        {
            return Result.Failure(AuthErrors.InvalidOrExpiredAccountAction);
        }

        User? user = await repository.GetUserByEmailAsync(
            EmailNormalizer.Normalize(command.Request.Email),
            cancellationToken);
        if (user is null || user.EmailVerificationStatus == EmailVerificationStatus.Verified)
        {
            return Result.Failure(AuthErrors.InvalidOrExpiredAccountAction);
        }

        AccountActionPurpose purpose = user.EmailVerificationStatus == EmailVerificationStatus.PendingVerification
            ? AccountActionPurpose.RegistrationEmailVerification
            : AccountActionPurpose.CurrentEmailVerification;
        AccountAction? action = await repository.GetLatestAccountActionAsync(user.Id, purpose, cancellationToken);
        DateTimeOffset now = timeProvider.GetUtcNow();

        if (action is null || !action.CanAttempt(now))
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

        if (user.EmailVerificationStatus == EmailVerificationStatus.PendingVerification)
        {
            foreach (IUserRegistrationInitializer initializer in registrationInitializers)
            {
                await initializer.InitializeAsync(UserId.From(user.Id), cancellationToken);
            }
        }

        user.VerifyEmail(now);
        action.MarkUsed(now);
        await repository.UpdateUserAsync(user, cancellationToken);
        await repository.UpdateAccountActionAsync(action, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
