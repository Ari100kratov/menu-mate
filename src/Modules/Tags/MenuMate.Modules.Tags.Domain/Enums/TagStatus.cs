namespace MenuMate.Modules.Tags.Domain.Enums;

/// <summary>
/// Статус подтверждения тега пользователем.
/// </summary>
public enum TagStatus
{
    /// <summary>
    /// Тег предложен, но еще не подтвержден.
    /// </summary>
    Proposed = 0,

    /// <summary>
    /// Тег подтвержден и может использоваться в обычных фильтрах.
    /// </summary>
    Confirmed = 1,

    /// <summary>
    /// Тег скрыт пользователем или администратором.
    /// </summary>
    Hidden = 2
}

