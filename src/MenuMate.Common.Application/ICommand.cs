using System.Diagnostics.CodeAnalysis;

namespace MenuMate.Common.Application;

/// <summary>
/// Маркер команд, которые изменяют состояние приложения.
/// </summary>
[SuppressMessage(
    "Design",
    "CA1040:Avoid empty interfaces",
    Justification = "CQRS command markers constrain handler registrations and pipeline contracts.")]
public interface ICommand;

/// <summary>
/// Маркер команд, которые возвращают значение.
/// </summary>
/// <typeparam name="TResponse">Тип ответа команды.</typeparam>
[SuppressMessage(
    "Design",
    "CA1040:Avoid empty interfaces",
    Justification = "CQRS command markers constrain handler registrations and pipeline contracts.")]
[SuppressMessage(
    "Major Code Smell",
    "S2326:Unused type parameters should be removed",
    Justification = "The response type is consumed by ICommandHandler generic constraints.")]
public interface ICommand<out TResponse>;
