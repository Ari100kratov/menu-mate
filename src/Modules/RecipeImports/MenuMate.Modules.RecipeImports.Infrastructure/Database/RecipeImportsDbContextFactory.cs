using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MenuMate.Modules.RecipeImports.Infrastructure.Database;

internal sealed class RecipeImportsDbContextFactory : IDesignTimeDbContextFactory<RecipeImportsDbContext>
{
    public RecipeImportsDbContext CreateDbContext(string[] args)
    {
        string connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Database")
            ?? "Host=localhost;Port=5432;Database=menumate;Username=menumate;Password=change-me";
        var optionsBuilder = new DbContextOptionsBuilder<RecipeImportsDbContext>();
        optionsBuilder.UseNpgsql(
                connectionString,
                options => options.MigrationsHistoryTable("__EFMigrationsHistory", RecipeImportsSchema.Name))
            .UseSnakeCaseNamingConvention();
        return new RecipeImportsDbContext(optionsBuilder.Options);
    }
}
