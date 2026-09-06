using MenuMate.Modules.Auth.Application;
using MenuMate.Modules.Auth.Application.Abstractions;
using Microsoft.Extensions.Options;

namespace MenuMate.Modules.Auth.Infrastructure.Legal;

internal sealed class PrivacyPolicyProvider(IOptions<LegalOptions> options) : IPrivacyPolicyProvider
{
    public PrivacyPolicyInfo Current { get; } = new(
        PrivacyPolicyDefaults.CurrentVersion,
        PrivacyPolicyDefaults.EffectiveAt,
        options.Value.OperatorName,
        options.Value.PrivacyContactEmail,
        options.Value.TechnicalLogRetentionDays,
        options.Value.BackupRetentionDays);
}
