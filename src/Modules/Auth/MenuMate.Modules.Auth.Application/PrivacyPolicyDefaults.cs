namespace MenuMate.Modules.Auth.Application;

/// <summary>Версия текста политики, встроенного в текущий клиент.</summary>
public static class PrivacyPolicyDefaults
{
    /// <summary>Идентификатор актуальной версии политики.</summary>
    public const string CurrentVersion = "2026-08-23";

    /// <summary>Дата вступления актуальной версии в силу.</summary>
    public static readonly DateTimeOffset EffectiveAt =
        new(2026, 8, 23, 0, 0, 0, TimeSpan.Zero);
}
