namespace MenuMate.DataImporter.Infrastructure.Database;

/// <summary>
/// Состояние импорта одной внешней страницы рецепта.
/// </summary>
public sealed class ImportItemRecord
{
    /// <summary>Идентификатор записи состояния.</summary>
    public Guid Id { get; set; }

    /// <summary>Код внешнего источника.</summary>
    public string Source { get; set; } = string.Empty;

    /// <summary>Идентификатор объекта во внешнем источнике.</summary>
    public string ExternalId { get; set; } = string.Empty;

    /// <summary>Идентификатор редакции внешнего объекта.</summary>
    public long SourceRevisionId { get; set; }

    /// <summary>Ссылка на исходную страницу.</summary>
    public Uri SourceUrl { get; set; } = null!;

    /// <summary>Хеш исходного содержимого.</summary>
    public string ContentHash { get; set; } = string.Empty;

    /// <summary>Идентификатор созданного рецепта.</summary>
    public Guid? RecipeId { get; set; }

    /// <summary>Текущий статус импорта.</summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>Последняя ошибка импорта.</summary>
    public string? LastError { get; set; }

    /// <summary>Момент последнего изменения состояния.</summary>
    public DateTimeOffset UpdatedAt { get; set; }
}
