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
        builder.HasKey(tag => new { tag.RecipeRevisionId, tag.TagId });
        builder.Property(tag => tag.TagId).ValueGeneratedNever();
        builder.HasIndex(tag => new { tag.TagId, tag.RecipeRevisionId });
    }
}
