using MenuMate.Modules.ShoppingLists.Infrastructure.Database.Source;
using MenuMate.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MenuMate.Modules.ShoppingLists.Infrastructure.Database.Configurations;

internal sealed class RecipeRevisionIngredientSourceRecordConfiguration
    : IEntityTypeConfiguration<RecipeRevisionIngredientSourceRecord>
{
    public void Configure(EntityTypeBuilder<RecipeRevisionIngredientSourceRecord> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("recipe_revision_ingredients", "recipes", table => table.ExcludeFromMigrations());
        builder.HasKey(ingredient => ingredient.Id);
        builder.Property(ingredient => ingredient.Id).ValueGeneratedNever();
        builder.Property(ingredient => ingredient.RecipeRevisionId)
            .HasConversion(revisionId => revisionId.Value, value => RecipeRevisionId.From(value));
        builder.Property(ingredient => ingredient.Unit).HasConversion<string>().HasMaxLength(64).IsRequired();
        builder.Property(ingredient => ingredient.Category).HasConversion<string>().HasMaxLength(64).IsRequired();
        builder.Property(ingredient => ingredient.ProductName).HasMaxLength(200).IsRequired();
        builder.Property(ingredient => ingredient.NormalizedProductName).HasMaxLength(200).IsRequired();
        builder.Property(ingredient => ingredient.Comment).HasMaxLength(500);
    }
}
