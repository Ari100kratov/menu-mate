namespace MenuMate.Modules.Auth.Application.Abstractions;

/// <summary>Предоставляет публичные реквизиты действующей политики конфиденциальности.</summary>
public interface IPrivacyPolicyProvider
{
    /// <summary>Возвращает реквизиты актуальной версии политики.</summary>
    PrivacyPolicyInfo Current { get; }
}

/// <summary>Реквизиты действующей политики конфиденциальности.</summary>
public sealed record PrivacyPolicyInfo(
    string Version,
    DateTimeOffset EffectiveAt,
    string OperatorName,
    string ContactEmail,
    int TechnicalLogRetentionDays,
    int BackupRetentionDays);
