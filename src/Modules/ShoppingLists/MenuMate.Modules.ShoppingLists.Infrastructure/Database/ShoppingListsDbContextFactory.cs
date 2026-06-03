using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MenuMate.Modules.ShoppingLists.Infrastructure.Database;

internal sealed class ShoppingListsDbContextFactory : IDesignTimeDbContextFactory<ShoppingListsDbContext>
{
    public ShoppingListsDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ShoppingListsDbContext>();
        optionsBuilder
            .UseNpgsql(
                "Host=localhost;Port=5432;Database=menumate;Username=postgres",
                npgsqlOptions => npgsqlOptions.MigrationsHistoryTable(
                    HistoryRepository.DefaultTableName,
                    ShoppingListsSchema.Name))
            .UseSnakeCaseNamingConvention();

        return new ShoppingListsDbContext(optionsBuilder.Options);
    }
}
