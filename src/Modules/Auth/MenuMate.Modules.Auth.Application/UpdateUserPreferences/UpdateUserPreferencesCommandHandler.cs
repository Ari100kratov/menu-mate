using MenuMate.Common.Application;
using MenuMate.Contracts.Auth;
using MenuMate.Modules.Auth.Application.Abstractions;
using MenuMate.Modules.Auth.Domain.Errors;
using MenuMate.Modules.Auth.Domain.Models;
using MenuMate.SharedKernel;

namespace MenuMate.Modules.Auth.Application.UpdateUserPreferences;

internal sealed class UpdateUserPreferencesCommandHandler(
    IAuthRepository repository,
    IAuthUnitOfWork unitOfWork,
    IUserContext userContext,
    TimeProvider timeProvider)
    : ICommandHandler<UpdateUserPreferencesCommand, UserProfileResponse>
{
    public async Task<Result<UserProfileResponse>> Handle(
        UpdateUserPreferencesCommand command,
        CancellationToken cancellationToken)
    {
        User? user = await repository.GetUserByIdAsync(userContext.UserId, cancellationToken);
        if (user is null)
        {
            return Result.Failure<UserProfileResponse>(AuthErrors.UserNotFound(userContext.UserId));
        }

        user.UpdatePreferences(command.Request.ShowShoppingListPreview, timeProvider.GetUtcNow());
        await repository.UpdateUserAsync(user, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return AuthMapping.ToProfile(user);
    }
}
