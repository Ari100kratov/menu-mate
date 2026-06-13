using MenuMate.Modules.MenuPlanning.Infrastructure.Database.Source;
using MenuMate.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MenuMate.Modules.MenuPlanning.Infrastructure.Database.Configurations;

internal sealed class RecipeCoverImageSourceRecordConfiguration
    : IEntityTypeConfiguration<RecipeCoverImageSourceRecord>
{
    public void Configure(EntityTypeBuilder<RecipeCoverImageSourceRecord> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.ToTable("recipe_images", "recipes", table => table.ExcludeFromMigrations());
        builder.HasKey(image => image.Id);
        builder.Property(image => image.RecipeId)
            .HasConversion(recipeId => recipeId.Value, value => RecipeId.From(value));
        builder.Property(image => image.Scope).HasColumnName("scope");
    }
}
