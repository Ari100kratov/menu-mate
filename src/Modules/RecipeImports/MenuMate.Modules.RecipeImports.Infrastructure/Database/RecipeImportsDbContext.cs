using MenuMate.Modules.RecipeImports.Application.Abstractions;
using MenuMate.Modules.RecipeImports.Infrastructure.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace MenuMate.Modules.RecipeImports.Infrastructure.Database;

/// <summary>
/// EF Core DbContext модуля Imports.
/// </summary>
public sealed class RecipeImportsDbContext(DbContextOptions<RecipeImportsDbContext> options)
    : DbContext(options), IRecipeImportsUnitOfWork
{
    internal DbSet<RecipeImportDraftRecord> RecipeImportDrafts => Set<RecipeImportDraftRecord>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.HasDefaultSchema(RecipeImportsSchema.Name);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(RecipeImportsDbContext).Assembly);
    }
}
