namespace MenuMate.Api;

internal sealed record HealthResponse(string Status, DateTimeOffset CheckedAt);

internal sealed record SystemResponse(string Name, string Version, string Description);

internal sealed record ModuleResponse(string Name, string Description);
