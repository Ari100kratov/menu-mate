using Microsoft.EntityFrameworkCore;

namespace MenuMate.DataImporter.Infrastructure.Database;

/// <summary>
/// Хранит состояние повторяемого первоначального импорта.
/// </summary>
public sealed class DataImportDbContext(DbContextOptions<DataImportDbContext> options) : DbContext(options)
{
    /// <summary>
    /// Импортированные внешние страницы.
    /// </summary>
    public DbSet<ImportItemRecord> ImportItems => Set<ImportItemRecord>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.HasDefaultSchema(DataImportSchema.Name);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DataImportDbContext).Assembly);
    }
}
