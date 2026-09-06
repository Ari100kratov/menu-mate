using MenuMate.Common.Application;
using MenuMate.Contracts.Auth;
using MenuMate.Modules.Auth.Application.Abstractions;
using MenuMate.SharedKernel;

namespace MenuMate.Modules.Auth.Application.GetPrivacyPolicy;

internal sealed class GetPrivacyPolicyQueryHandler(IPrivacyPolicyProvider provider)
    : IQueryHandler<GetPrivacyPolicyQuery, PrivacyPolicyResponse>
{
    public Task<Result<PrivacyPolicyResponse>> Handle(
        GetPrivacyPolicyQuery query,
        CancellationToken cancellationToken)
    {
        PrivacyPolicyInfo policy = provider.Current;
        return Task.FromResult<Result<PrivacyPolicyResponse>>(new PrivacyPolicyResponse(
            policy.Version,
            policy.EffectiveAt,
            policy.OperatorName,
            policy.ContactEmail,
            policy.TechnicalLogRetentionDays,
            policy.BackupRetentionDays));
    }
}
