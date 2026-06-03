using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MenuMate.Modules.Recipes.Infrastructure.Database;

internal sealed class RecipesDbContextFactory : IDesignTimeDbContextFactory<RecipesDbContext>
{
    public RecipesDbContext CreateDbContext(string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);

        var optionsBuilder = new DbContextOptionsBuilder<RecipesDbContext>();
        optionsBuilder
            .UseNpgsql(
                "Host=localhost;Port=5432;Database=menumate;Username=postgres",
                npgsqlOptions => npgsqlOptions.MigrationsHistoryTable(
                    HistoryRepository.DefaultTableName,
                    RecipesSchema.Name))
            .UseSnakeCaseNamingConvention();

        return new RecipesDbContext(optionsBuilder.Options);
    }
}
