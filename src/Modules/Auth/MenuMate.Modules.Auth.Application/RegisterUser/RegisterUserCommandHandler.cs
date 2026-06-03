using MenuMate.Common.Application;
using MenuMate.Contracts.Auth;
using MenuMate.Modules.Auth.Application;
using MenuMate.Modules.Auth.Application.Abstractions;
using MenuMate.Modules.Auth.Domain.Errors;
using MenuMate.Modules.Auth.Domain.Models;
using MenuMate.Modules.Auth.Domain.ValueObjects;
using MenuMate.SharedKernel;

namespace MenuMate.Modules.Auth.Application.RegisterUser;

internal sealed class RegisterUserCommandHandler(
    IAuthRepository repository,
    IAuthUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher,
    ITokenProvider tokenProvider,
    IRefreshTokenService refreshTokenService,
    TimeProvider timeProvider)
    : ICommandHandler<RegisterUserCommand, RegisterUserSession>
{
    public async Task<Result<RegisterUserSession>> Handle(
        RegisterUserCommand command,
        CancellationToken cancellationToken)
    {
        string email = EmailNormalizer.Normalize(command.Request.Email);
        if (await repository.EmailExistsAsync(email, cancellationToken))
        {
            return Result.Failure<RegisterUserSession>(AuthErrors.EmailNotUnique);
        }

        if (string.IsNullOrWhiteSpace(command.Request.Password))
        {
            return Result.Failure<RegisterUserSession>(AuthErrors.EmptyPassword);
        }

        Result<User> userResult = User.Create(
            Guid.CreateVersion7(),
            email,
            command.Request.DisplayName,
            passwordHasher.Hash(command.Request.Password),
            timeProvider.GetUtcNow());

        if (userResult.IsFailure)
        {
            return Result.Failure<RegisterUserSession>(userResult.Error);
        }

        Role role = await repository.GetRoleByNameAsync(AuthRoleNames.User, cancellationToken)
            ?? throw new InvalidOperationException("Default user role was not found.");

        User user = userResult.Value;
        user.AddRole(role.Id, role.Name);

        await repository.AddUserAsync(user, cancellationToken);

        AccessToken accessToken = tokenProvider.Create(user);
        RefreshToken refreshToken = refreshTokenService.Generate(user);

        await repository.AddRefreshTokenAsync(refreshToken, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new RegisterUserSession(
            AuthMapping.ToProfile(user),
            AuthMapping.ToTokenResponse(accessToken),
            refreshToken);
    }
}
