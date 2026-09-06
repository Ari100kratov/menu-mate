using MenuMate.Common.Application;
using MenuMate.Contracts.Auth;

namespace MenuMate.Modules.Auth.Application.UpdateDisplayName;

internal sealed record UpdateDisplayNameCommand(UpdateDisplayNameRequest Request) : ICommand<UserProfileResponse>;
