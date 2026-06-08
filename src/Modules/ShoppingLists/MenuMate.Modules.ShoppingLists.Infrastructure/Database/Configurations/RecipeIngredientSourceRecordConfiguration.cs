using MenuMate.Modules.ShoppingLists.Infrastructure.Database.Source;
using MenuMate.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MenuMate.Modules.ShoppingLists.Infrastructure.Database.Configurations;

internal sealed class RecipeIngredientSourceRecordConfiguration : IEntityTypeConfiguration<RecipeIngredientSourceRecord>
{
    public void Configure(EntityTypeBuilder<RecipeIngredientSourceRecord> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("recipe_ingredients", "recipes", table => table.ExcludeFromMigrations());
        builder.HasKey(ingredient => ingredient.Id);
        builder.Property(ingredient => ingredient.Id).ValueGeneratedNever();
        builder.Property(ingredient => ingredient.IngredientId).IsRequired();
        builder.Property(ingredient => ingredient.RecipeId)
            .HasConversion(recipeId => recipeId.Value, value => RecipeId.From(value));
        builder.Property(ingredient => ingredient.Unit).HasConversion<string>().HasMaxLength(64).IsRequired();
        builder.Property(ingredient => ingredient.QuantityKind).HasConversion<string>().HasMaxLength(64).IsRequired();
        builder.Property(ingredient => ingredient.Category).HasConversion<string>().HasMaxLength(64).IsRequired();
        builder.Property(ingredient => ingredient.ProductName).HasMaxLength(200).IsRequired();
        builder.Property(ingredient => ingredient.NormalizedProductName).HasMaxLength(200).IsRequired();
        builder.Property(ingredient => ingredient.Comment).HasMaxLength(500);
    }
}
