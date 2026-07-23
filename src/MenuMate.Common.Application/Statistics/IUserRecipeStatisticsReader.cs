namespace MenuMate.Common.Application.Statistics;

/// <summary>
/// Предоставляет агрегированные данные о рецептах пользователей для межмодульных сценариев чтения.
/// </summary>
public interface IUserRecipeStatisticsReader
{
    /// <summary>
    /// Возвращает количество активных рецептов каждого указанного пользователя.
    /// </summary>
    Task<IReadOnlyDictionary<Guid, int>> GetActiveRecipeCountsByOwnerAsync(
        IReadOnlyCollection<Guid> userIds,
        CancellationToken cancellationToken);
}
