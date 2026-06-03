using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MenuMate.Modules.MenuPlanning.Infrastructure.Database;

internal sealed class MenuPlanningDbContextFactory : IDesignTimeDbContextFactory<MenuPlanningDbContext>
{
    public MenuPlanningDbContext CreateDbContext(string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);

        var optionsBuilder = new DbContextOptionsBuilder<MenuPlanningDbContext>();
        optionsBuilder
            .UseNpgsql(
                "Host=localhost;Port=5432;Database=menumate;Username=postgres",
                npgsqlOptions => npgsqlOptions.MigrationsHistoryTable(
                    HistoryRepository.DefaultTableName,
                    MenuPlanningSchema.Name))
            .UseSnakeCaseNamingConvention();

        return new MenuPlanningDbContext(optionsBuilder.Options);
    }
}
