using MenuMate.Common.Application;
using MenuMate.Contracts.Auth;

namespace MenuMate.Modules.Auth.Application.UpdateUserPreferences;

internal sealed record UpdateUserPreferencesCommand(UpdateUserPreferencesRequest Request)
    : ICommand<UserProfileResponse>;
