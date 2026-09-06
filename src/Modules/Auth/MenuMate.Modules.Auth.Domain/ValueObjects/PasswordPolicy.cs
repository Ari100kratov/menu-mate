namespace MenuMate.Modules.Auth.Domain.ValueObjects;

/// <summary>
/// Единая политика длины пароля для всех сценариев Auth.
/// </summary>
public static class PasswordPolicy
{
    /// <summary>Минимальная длина пароля.</summary>
    public const int MinimumLength = 8;

    /// <summary>Максимальная длина пароля.</summary>
    public const int MaximumLength = 128;

    /// <summary>Проверяет пароль без требований к составу символов.</summary>
    public static bool IsValid(string? password) =>
        password?.Length is >= MinimumLength and <= MaximumLength;
}
