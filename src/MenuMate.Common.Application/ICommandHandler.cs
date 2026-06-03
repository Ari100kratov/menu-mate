using MenuMate.SharedKernel;

namespace MenuMate.Common.Application;

/// <summary>
/// Обработчик команды без возвращаемого значения.
/// </summary>
/// <typeparam name="TCommand">Тип команды.</typeparam>
public interface ICommandHandler<in TCommand>
    where TCommand : ICommand
{
    /// <summary>
    /// Выполняет команду.
    /// </summary>
    Task<Result> Handle(TCommand command, CancellationToken cancellationToken);
}

/// <summary>
/// Обработчик команды с возвращаемым значением.
/// </summary>
/// <typeparam name="TCommand">Тип команды.</typeparam>
/// <typeparam name="TResponse">Тип ответа.</typeparam>
public interface ICommandHandler<in TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
    /// <summary>
    /// Выполняет команду.
    /// </summary>
    Task<Result<TResponse>> Handle(TCommand command, CancellationToken cancellationToken);
}

