using MenuMate.Common.Application;
using MenuMate.Contracts.Auth;
using MenuMate.Modules.Auth.Application.Abstractions;
using MenuMate.Modules.Auth.Domain.Errors;
using MenuMate.Modules.Auth.Domain.Models;
using MenuMate.SharedKernel;

namespace MenuMate.Modules.Auth.Application.AcceptPrivacyPolicy;

internal sealed class AcceptPrivacyPolicyCommandHandler(
    IAuthRepository repository,
    IAuthUnitOfWork unitOfWork,
    IUserContext userContext,
    IPrivacyPolicyProvider privacyPolicyProvider,
    TimeProvider timeProvider)
    : ICommandHandler<AcceptPrivacyPolicyCommand, UserProfileResponse>
{
    public async Task<Result<UserProfileResponse>> Handle(
        AcceptPrivacyPolicyCommand command,
        CancellationToken cancellationToken)
    {
        if (command.Request.PrivacyPolicyVersion != privacyPolicyProvider.Current.Version)
        {
            return Result.Failure<UserProfileResponse>(AuthErrors.PrivacyPolicyOutdated);
        }

        User? user = await repository.GetUserByIdAsync(userContext.UserId, cancellationToken);
        if (user is null)
        {
            return Result.Failure<UserProfileResponse>(AuthErrors.UserNotFound(userContext.UserId));
        }

        user.AcceptPrivacyPolicy(privacyPolicyProvider.Current.Version, timeProvider.GetUtcNow());
        await repository.UpdateUserAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return AuthMapping.ToProfile(user);
    }
}
