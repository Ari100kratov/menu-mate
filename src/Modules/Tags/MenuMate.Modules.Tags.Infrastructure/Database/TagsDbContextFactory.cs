using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MenuMate.Modules.Tags.Infrastructure.Database;

internal sealed class TagsDbContextFactory : IDesignTimeDbContextFactory<TagsDbContext>
{
    public TagsDbContext CreateDbContext(string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);

        var optionsBuilder = new DbContextOptionsBuilder<TagsDbContext>();
        optionsBuilder
            .UseNpgsql(
                "Host=localhost;Port=5432;Database=menumate;Username=postgres",
                npgsqlOptions => npgsqlOptions.MigrationsHistoryTable(
                    HistoryRepository.DefaultTableName,
                    TagsSchema.Name))
            .UseSnakeCaseNamingConvention();

        return new TagsDbContext(optionsBuilder.Options);
    }
}
