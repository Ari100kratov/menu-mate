using MenuMate.Modules.ShoppingLists.Infrastructure.Database.Source;
using MenuMate.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MenuMate.Modules.ShoppingLists.Infrastructure.Database.Configurations;

internal sealed class RecipeRevisionSourceRecordConfiguration : IEntityTypeConfiguration<RecipeRevisionSourceRecord>
{
    public void Configure(EntityTypeBuilder<RecipeRevisionSourceRecord> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("recipe_revisions", "recipes", table => table.ExcludeFromMigrations());
        builder.HasKey(revision => revision.Id);
        builder.Property(revision => revision.Id)
            .ValueGeneratedNever()
            .HasConversion(revisionId => revisionId.Value, value => RecipeRevisionId.From(value));
        builder.Property(revision => revision.RecipeId)
            .HasConversion(recipeId => recipeId.Value, value => RecipeId.From(value));
        builder.HasMany(revision => revision.Ingredients)
            .WithOne()
            .HasForeignKey(ingredient => ingredient.RecipeRevisionId);
    }
}
