using MenuMate.Modules.MenuPlanning.Infrastructure.Database.Source;
using MenuMate.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MenuMate.Modules.MenuPlanning.Infrastructure.Database.Configurations;

internal sealed class RecipeRevisionAccessSourceRecordConfiguration
    : IEntityTypeConfiguration<RecipeRevisionAccessSourceRecord>
{
    public void Configure(EntityTypeBuilder<RecipeRevisionAccessSourceRecord> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.ToTable("recipe_revisions", "recipes", table => table.ExcludeFromMigrations());
        builder.HasKey(revision => revision.Id);
        builder.Property(revision => revision.Id)
            .HasConversion(revisionId => revisionId.Value, value => RecipeRevisionId.From(value));
        builder.Property(revision => revision.RecipeId)
            .HasConversion(recipeId => recipeId.Value, value => RecipeId.From(value));
        builder.Property(revision => revision.Title).HasMaxLength(160).IsRequired();
    }
}
