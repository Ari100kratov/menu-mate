using MenuMate.Modules.Recipes.Infrastructure.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MenuMate.Modules.Recipes.Infrastructure.Database.Configurations;

internal sealed class RecipeRevisionIngredientRecordConfiguration : IEntityTypeConfiguration<RecipeRevisionIngredientRecord>
{
    public void Configure(EntityTypeBuilder<RecipeRevisionIngredientRecord> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("recipe_revision_ingredients");
        builder.HasKey(ingredient => ingredient.Id);
        builder.Property(ingredient => ingredient.Id).ValueGeneratedNever();
        builder.Property(ingredient => ingredient.IngredientId).IsRequired();
        builder.Property(ingredient => ingredient.Unit).HasConversion<string>().HasMaxLength(64).IsRequired();
        builder.Property(ingredient => ingredient.Category).HasConversion<string>().HasMaxLength(64).IsRequired();
        builder.Property(ingredient => ingredient.ProductName).HasMaxLength(200).IsRequired();
        builder.Property(ingredient => ingredient.NormalizedProductName).HasMaxLength(200).IsRequired();
        builder.Property(ingredient => ingredient.Comment).HasMaxLength(500);
        builder.HasIndex(ingredient => new { ingredient.RecipeRevisionId, ingredient.Order }).IsUnique();
    }
}
