namespace MenuMate.Common.Application.Tags;

/// <summary>
/// Общий глобальный каталог тегов.
/// </summary>
public interface ITagCatalog
{
    /// <summary>
    /// Возвращает существующие теги или атомарно создаёт отсутствующие.
    /// </summary>
    Task<IReadOnlyCollection<TagCatalogItem>> ResolveAsync(
        IReadOnlyCollection<string> names,
        TagCatalogSource source,
        CancellationToken cancellationToken);

    /// <summary>
    /// Возвращает отображаемые названия тегов по идентификаторам.
    /// </summary>
    Task<IReadOnlyDictionary<Guid, string>> GetNamesAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken);
}

/// <summary>
/// Тег из глобального каталога.
/// </summary>
/// <param name="Id">Идентификатор тега.</param>
/// <param name="Name">Каноническое отображаемое название.</param>
public sealed record TagCatalogItem(Guid Id, string Name);

/// <summary>
/// Источник появления тега в общем каталоге.
/// </summary>
public enum TagCatalogSource
{
    /// <summary>Тег введён пользователем вручную.</summary>
    User = 0,

    /// <summary>Тег предложен автоматикой или импортом.</summary>
    Suggested = 1
}
