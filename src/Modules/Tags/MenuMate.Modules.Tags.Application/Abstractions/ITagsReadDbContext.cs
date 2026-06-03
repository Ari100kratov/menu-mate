using MenuMate.Contracts.Tags;

namespace MenuMate.Modules.Tags.Application.Abstractions;

/// <summary>
/// Контракт чтения тегов через EF-проекции без гидрации доменных агрегатов.
/// </summary>
internal interface ITagsReadDbContext
{
    /// <summary>
    /// Возвращает теги с фильтрацией для внешнего API.
    /// </summary>
    Task<IReadOnlyCollection<TagResponse>> GetTagsAsync(
        string? search,
        bool includeHidden,
        CancellationToken cancellationToken);
}
