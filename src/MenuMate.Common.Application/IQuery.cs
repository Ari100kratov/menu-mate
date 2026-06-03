using System.Diagnostics.CodeAnalysis;

namespace MenuMate.Common.Application;

/// <summary>
/// Маркер запросов, которые не изменяют состояние приложения.
/// </summary>
/// <typeparam name="TResponse">Тип ответа запроса.</typeparam>
[SuppressMessage(
    "Design",
    "CA1040:Avoid empty interfaces",
    Justification = "CQRS query markers constrain handler registrations and pipeline contracts.")]
[SuppressMessage(
    "Major Code Smell",
    "S2326:Unused type parameters should be removed",
    Justification = "The response type is consumed by IQueryHandler generic constraints.")]
public interface IQuery<out TResponse>;
