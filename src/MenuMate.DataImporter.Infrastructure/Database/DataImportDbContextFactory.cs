using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MenuMate.DataImporter.Infrastructure.Database;

/// <summary>
/// Создает контекст состояния импорта для EF Core CLI.
/// </summary>
public sealed class DataImportDbContextFactory : IDesignTimeDbContextFactory<DataImportDbContext>
{
    /// <inheritdoc />
    public DataImportDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DataImportDbContext>();
        optionsBuilder
            .UseNpgsql(
                "Host=localhost;Port=5432;Database=menumate;Username=postgres",
                npgsqlOptions => npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", DataImportSchema.Name))
            .UseSnakeCaseNamingConvention();

        return new DataImportDbContext(optionsBuilder.Options);
    }
}
