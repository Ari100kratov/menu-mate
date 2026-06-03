using MenuMate.Modules.Recipes.Infrastructure.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MenuMate.Modules.Recipes.Infrastructure.Database.Configurations;

internal sealed class RecipeTagRecordConfiguration : IEntityTypeConfiguration<RecipeTagRecord>
{
    public void Configure(EntityTypeBuilder<RecipeTagRecord> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("recipe_tags");
        builder.HasKey(tag => tag.Id);
        builder.Property(tag => tag.Id).ValueGeneratedNever();
        builder.Property(tag => tag.Value).HasMaxLength(64).IsRequired();
        builder.Property(tag => tag.NormalizedValue).HasMaxLength(64).IsRequired();
        builder.HasIndex(tag => tag.NormalizedValue);
    }
}
