using System.Diagnostics.CodeAnalysis;

namespace MenuMate.Modules.Auth.Domain.ValueObjects;

/// <summary>
/// Нормализует email для входа и проверки уникальности.
/// </summary>
public static class EmailNormalizer
{
    /// <summary>
    /// Возвращает обрезанный ключ email без учета регистра.
    /// </summary>
    [SuppressMessage(
        "Globalization",
        "CA1308:Normalize strings to uppercase",
        Justification = "Email addresses are intentionally stored and compared in lowercase for user-facing consistency.")]
    public static string Normalize(string email)
    {
        ArgumentNullException.ThrowIfNull(email);

        return email.Trim().ToLowerInvariant();
    }
}
