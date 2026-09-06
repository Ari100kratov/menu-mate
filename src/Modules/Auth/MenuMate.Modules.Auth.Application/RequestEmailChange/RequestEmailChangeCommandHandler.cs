using MenuMate.Common.Application;
using MenuMate.Modules.Auth.Application.Abstractions;
using MenuMate.Modules.Auth.Domain.Errors;
using MenuMate.Modules.Auth.Domain.Models;
using MenuMate.Modules.Auth.Domain.ValueObjects;
using MenuMate.SharedKernel;
using Microsoft.Extensions.Logging;

namespace MenuMate.Modules.Auth.Application.RequestEmailChange;

internal sealed class RequestEmailChangeCommandHandler(
    IAuthRepository repository,
    IAuthUnitOfWork unitOfWork,
    IUserContext userContext,
    IPasswordHasher passwordHasher,
    IAccountActionTokenService actionTokenService,
    IAuthEmailSender emailSender,
    TimeProvider timeProvider,
    ILogger<RequestEmailChangeCommandHandler> logger)
    : ICommandHandler<RequestEmailChangeCommand>
{
    public async Task<Result> Handle(
        RequestEmailChangeCommand command,
        CancellationToken cancellationToken)
    {
        Result validation = AuthInputValidator.ValidateEmail(command.Request.NewEmail);
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

        string newEmail = EmailNormalizer.Normalize(command.Request.NewEmail);
        if (newEmail == user.Email || await repository.EmailExistsAsync(newEmail, cancellationToken))
        {
            return Result.Failure(AuthErrors.EmailNotUnique);
        }

        DateTimeOffset now = timeProvider.GetUtcNow();
        AccountAction? previous = await repository.GetLatestAccountActionAsync(
            user.Id,
            AccountActionPurpose.EmailChange,
            cancellationToken);
        if (previous is not null && previous.UsedAt is null && previous.ResendAvailableAt > now)
        {
            return Result.Failure(AuthErrors.ResendNotAvailable);
        }

        await repository.InvalidateAccountActionsAsync(
            user.Id,
            AccountActionPurpose.EmailChange,
            now,
            cancellationToken);
        string code = actionTokenService.GenerateCode();
        DateTimeOffset expiresAt = now.Add(AccountActionPolicy.VerificationCodeLifetime);
        var action = AccountAction.Create(
            Guid.CreateVersion7(),
            user.Id,
            AccountActionPurpose.EmailChange,
            newEmail,
            actionTokenService.Hash(code),
            now,
            expiresAt,
            now.Add(AccountActionPolicy.ResendCooldown));
        await repository.AddAccountActionAsync(action, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        try
        {
            await emailSender.SendEmailChangeCodeAsync(newEmail, code, expiresAt, cancellationToken);
        }
        catch (AuthEmailDeliveryException exception)
        {
            AuthApplicationLogMessages.EmailDeliveryFailed(logger, "email change request", exception);
            return Result.Failure(AuthErrors.EmailDeliveryFailed);
        }

        return Result.Success();
    }
}
