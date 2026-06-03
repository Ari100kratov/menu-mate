using MenuMate.Modules.ShoppingLists.Infrastructure.Database.Source;
using MenuMate.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MenuMate.Modules.ShoppingLists.Infrastructure.Database.Configurations;

internal sealed class RecipeSourceRecordConfiguration : IEntityTypeConfiguration<RecipeSourceRecord>
{
    public void Configure(EntityTypeBuilder<RecipeSourceRecord> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("recipes", "recipes", table => table.ExcludeFromMigrations());
        builder.HasKey(recipe => recipe.Id);
        builder.Property(recipe => recipe.Id)
            .ValueGeneratedNever()
            .HasConversion(recipeId => recipeId.Value, value => RecipeId.From(value));
        builder.Property(recipe => recipe.OwnerUserId)
            .HasConversion(userId => userId.Value, value => UserId.From(value))
            .IsRequired();

        builder.HasMany(recipe => recipe.Ingredients)
            .WithOne()
            .HasForeignKey(ingredient => ingredient.RecipeId);
    }
}
