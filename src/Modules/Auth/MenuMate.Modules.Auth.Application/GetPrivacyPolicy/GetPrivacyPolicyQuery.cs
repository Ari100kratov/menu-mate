using MenuMate.Common.Application;
using MenuMate.Contracts.Auth;

namespace MenuMate.Modules.Auth.Application.GetPrivacyPolicy;

internal sealed record GetPrivacyPolicyQuery : IQuery<PrivacyPolicyResponse>;
