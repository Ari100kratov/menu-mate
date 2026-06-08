using MenuMate.Modules.Recipes.Infrastructure.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MenuMate.Modules.Recipes.Infrastructure.Database.Configurations;

internal sealed class RecipeRevisionTagRecordConfiguration : IEntityTypeConfiguration<RecipeRevisionTagRecord>
{
    public void Configure(EntityTypeBuilder<RecipeRevisionTagRecord> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("recipe_revision_tags");
        builder.HasKey(tag => tag.Id);
        builder.Property(tag => tag.Id).ValueGeneratedNever();
        builder.Property(tag => tag.Value).HasMaxLength(64).IsRequired();
        builder.Property(tag => tag.NormalizedValue).HasMaxLength(64).IsRequired();
        builder.HasIndex(tag => new { tag.RecipeRevisionId, tag.NormalizedValue }).IsUnique();
    }
}
