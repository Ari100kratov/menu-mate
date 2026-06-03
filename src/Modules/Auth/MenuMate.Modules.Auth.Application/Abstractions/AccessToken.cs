namespace MenuMate.Modules.Auth.Application.Abstractions;

/// <summary>
/// Выданный токен доступа.
/// </summary>
/// <param name="Value">Значение токена.</param>
/// <param name="ExpiresAt">Момент истечения срока действия.</param>
public sealed record AccessToken(string Value, DateTimeOffset ExpiresAt);
