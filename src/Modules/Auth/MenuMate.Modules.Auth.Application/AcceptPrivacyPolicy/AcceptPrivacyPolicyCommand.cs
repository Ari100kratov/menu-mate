using MenuMate.Common.Application;
using MenuMate.Contracts.Auth;

namespace MenuMate.Modules.Auth.Application.AcceptPrivacyPolicy;

internal sealed record AcceptPrivacyPolicyCommand(AcceptPrivacyPolicyRequest Request)
    : ICommand<UserProfileResponse>;
