namespace MenuMate.Contracts.Tags;

/// <summary>
/// Тег, возвращаемый Tags API.
/// </summary>
/// <param name="Id">Идентификатор тега.</param>
/// <param name="Name">Отображаемое имя тега.</param>
/// <param name="NormalizedName">Нормализованное имя тега.</param>
/// <param name="Kind">Источник тега.</param>
/// <param name="Status">Статус подтверждения тега.</param>
/// <param name="CreatedAt">Момент создания.</param>
/// <param name="UpdatedAt">Момент последнего изменения.</param>
public sealed record TagResponse(
    Guid Id,
    string Name,
    string NormalizedName,
    string Kind,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

/// <summary>
/// Запрос на создание тега.
/// </summary>
/// <param name="Name">Отображаемое имя тега.</param>
/// <param name="Kind">Необязательный источник тега.</param>
public sealed record CreateTagRequest(string Name, string? Kind);
