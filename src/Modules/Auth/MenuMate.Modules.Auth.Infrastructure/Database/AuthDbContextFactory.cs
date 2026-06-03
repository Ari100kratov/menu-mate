using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MenuMate.Modules.Auth.Infrastructure.Database;

internal sealed class AuthDbContextFactory : IDesignTimeDbContextFactory<AuthDbContext>
{
    public AuthDbContext CreateDbContext(string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);

        var optionsBuilder = new DbContextOptionsBuilder<AuthDbContext>();
        optionsBuilder
            .UseNpgsql(
                "Host=localhost;Port=5432;Database=menumate;Username=postgres",
                npgsqlOptions => npgsqlOptions.MigrationsHistoryTable(
                    HistoryRepository.DefaultTableName,
                    AuthSchema.Name))
            .UseSnakeCaseNamingConvention();

        return new AuthDbContext(optionsBuilder.Options);
    }
}
