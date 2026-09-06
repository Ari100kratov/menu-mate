using MenuMate.Common.Application;
using MenuMate.Contracts.Auth;
using MenuMate.Modules.Auth.Application;
using MenuMate.Modules.Auth.Application.Abstractions;
using MenuMate.Modules.Auth.Domain.Errors;
using MenuMate.Modules.Auth.Domain.Models;
using MenuMate.Modules.Auth.Domain.ValueObjects;
using MenuMate.SharedKernel;

namespace MenuMate.Modules.Auth.Application.LoginUser;

internal sealed class LoginUserCommandHandler(
    IAuthRepository repository,
    IAuthUnitOfWork unitOfWork,
    IPasswordHasher passwordHasher,
    ITokenProvider tokenProvider,
    IRefreshTokenService refreshTokenService)
    : ICommandHandler<LoginUserCommand, AuthSession>
{
    public async Task<Result<AuthSession>> Handle(LoginUserCommand command, CancellationToken cancellationToken)
    {
        if (AuthInputValidator.ValidateEmail(command.Request.Email).IsFailure ||
            string.IsNullOrEmpty(command.Request.Password))
        {
            return Result.Failure<AuthSession>(AuthErrors.InvalidCredentials);
        }

        User? user = await repository.GetUserByEmailAsync(
            EmailNormalizer.Normalize(command.Request.Email),
            cancellationToken);

        if (user is null || !passwordHasher.Verify(command.Request.Password, user.PasswordHash))
        {
            return Result.Failure<AuthSession>(AuthErrors.InvalidCredentials);
        }

        if (user.EmailVerificationStatus == EmailVerificationStatus.PendingVerification)
        {
            return Result.Failure<AuthSession>(AuthErrors.EmailNotVerified);
        }

        AccessToken accessToken = tokenProvider.Create(user);
        RefreshToken refreshToken = refreshTokenService.Generate(user);

        await repository.AddRefreshTokenAsync(refreshToken, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new AuthSession(AuthMapping.ToTokenResponse(accessToken), refreshToken);
    }
}
