using MenuMate.Common.Application;
using MenuMate.Contracts.Auth;
using MenuMate.Modules.Auth.Application.Abstractions;
using MenuMate.Modules.Auth.Domain.Errors;
using MenuMate.Modules.Auth.Domain.Models;
using MenuMate.Modules.Auth.Domain.ValueObjects;
using MenuMate.SharedKernel;
using Microsoft.Extensions.Logging;

namespace MenuMate.Modules.Auth.Application.RegisterUser;

internal sealed class RegisterUserCommandHandler(
    IAuthRepository repository,
    IAuthUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher,
    IAccountActionTokenService actionTokenService,
    IAuthEmailSender emailSender,
    IPrivacyPolicyProvider privacyPolicyProvider,
    TimeProvider timeProvider,
    ILogger<RegisterUserCommandHandler> logger)
    : ICommandHandler<RegisterUserCommand, RegisterUserResponse>
{
    public async Task<Result<RegisterUserResponse>> Handle(
        RegisterUserCommand command,
        CancellationToken cancellationToken)
    {
        Result validation = AuthInputValidator.ValidateEmail(command.Request.Email);
        if (validation.IsFailure)
        {
            return Result.Failure<RegisterUserResponse>(validation.Error);
        }

        validation = AuthInputValidator.ValidateDisplayName(command.Request.DisplayName);
        if (validation.IsFailure)
        {
            return Result.Failure<RegisterUserResponse>(validation.Error);
        }

        validation = AuthInputValidator.ValidatePassword(command.Request.Password);
        if (validation.IsFailure)
        {
            return Result.Failure<RegisterUserResponse>(validation.Error);
        }

        if (command.Request.PrivacyPolicyVersion != privacyPolicyProvider.Current.Version)
        {
            return Result.Failure<RegisterUserResponse>(AuthErrors.PrivacyPolicyOutdated);
        }

        string email = EmailNormalizer.Normalize(command.Request.Email);
        if (await repository.EmailExistsAsync(email, cancellationToken))
        {
            return Result.Failure<RegisterUserResponse>(AuthErrors.EmailNotUnique);
        }

        DateTimeOffset now = timeProvider.GetUtcNow();
        Result<User> userResult = User.Create(
            Guid.CreateVersion7(),
            email,
            command.Request.DisplayName,
            passwordHasher.Hash(command.Request.Password),
            privacyPolicyProvider.Current.Version,
            now);

        if (userResult.IsFailure)
        {
            return Result.Failure<RegisterUserResponse>(userResult.Error);
        }

        Role role = await repository.GetRoleByNameAsync(AuthRoleNames.User, cancellationToken)
            ?? throw new InvalidOperationException("Default user role was not found.");

        User user = userResult.Value;
        user.AddRole(role.Id, role.Name);

        string code = actionTokenService.GenerateCode();
        DateTimeOffset expiresAt = now.Add(AccountActionPolicy.VerificationCodeLifetime);
        DateTimeOffset resendAvailableAt = now.Add(AccountActionPolicy.ResendCooldown);
        var action = AccountAction.Create(
            Guid.CreateVersion7(),
            user.Id,
            AccountActionPurpose.RegistrationEmailVerification,
            email,
            actionTokenService.Hash(code),
            now,
            expiresAt,
            resendAvailableAt);

        await repository.AddUserAsync(user, cancellationToken);
        await repository.AddAccountActionAsync(action, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        try
        {
            await emailSender.SendVerificationCodeAsync(email, code, expiresAt, cancellationToken);
        }
        catch (AuthEmailDeliveryException exception)
        {
            AuthApplicationLogMessages.EmailDeliveryFailed(logger, "registration verification", exception);
            return Result.Failure<RegisterUserResponse>(AuthErrors.RegistrationEmailDeliveryFailed);
        }

        return new RegisterUserResponse(email, expiresAt, resendAvailableAt);
    }
}
