using MenuMate.SharedKernel;

namespace MenuMate.Common.Application;

/// <summary>
/// Обработчик запроса.
/// </summary>
/// <typeparam name="TQuery">Тип запроса.</typeparam>
/// <typeparam name="TResponse">Тип ответа.</typeparam>
public interface IQueryHandler<in TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
    /// <summary>
    /// Выполняет запрос.
    /// </summary>
    Task<Result<TResponse>> Handle(TQuery query, CancellationToken cancellationToken);
}

