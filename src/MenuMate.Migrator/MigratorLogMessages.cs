using Microsoft.Extensions.Logging;

namespace MenuMate.Migrator;

internal static partial class MigratorLogMessages
{
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "Starting MenuMate database migrations.")]
    public static partial void StartingMigrations(ILogger logger);

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Information,
        Message = "MenuMate database migrations completed.")]
    public static partial void MigrationsCompleted(ILogger logger);

    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Information,
        Message = "Applying EF Core migrations for {DbContextName}.")]
    public static partial void StartingDbContextMigration(ILogger logger, string dbContextName);

    [LoggerMessage(
        EventId = 4,
        Level = LogLevel.Information,
        Message = "EF Core migrations for {DbContextName} completed.")]
    public static partial void DbContextMigrationCompleted(ILogger logger, string dbContextName);
}
