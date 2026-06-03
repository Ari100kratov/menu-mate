namespace MenuMate.Common.Presentation;

/// <summary>
/// Единый формат ошибки HTTP API.
/// </summary>
/// <param name="Code">Машиночитаемый код ошибки.</param>
/// <param name="Message">Сообщение для человека.</param>
/// <param name="TraceId">Идентификатор трассировки запроса.</param>
public sealed record ApiErrorResponse(string Code, string Message, string? TraceId);

