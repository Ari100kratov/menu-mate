namespace MenuMate.Modules.Tags.Domain.Enums;

/// <summary>
/// Источник появления тега в системе.
/// </summary>
public enum TagKind
{
    /// <summary>
    /// Системный тег, поставляемый приложением.
    /// </summary>
    System = 0,

    /// <summary>
    /// Пользовательский тег.
    /// </summary>
    User = 1,

    /// <summary>
    /// Тег, предложенный автоматикой или импортом.
    /// </summary>
    Suggested = 2
}

