using MenuMate.Modules.Recipes.Infrastructure.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MenuMate.Modules.Recipes.Infrastructure.Database.Configurations;

internal sealed class RecipeRevisionRecordConfiguration : IEntityTypeConfiguration<RecipeRevisionRecord>
{
    public void Configure(EntityTypeBuilder<RecipeRevisionRecord> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("recipe_revisions");
        builder.HasKey(revision => revision.Id);
        builder.Property(revision => revision.Id).ValueGeneratedNever();
        builder.Property(revision => revision.Title).HasMaxLength(160).IsRequired();
        builder.Property(revision => revision.Description).HasMaxLength(2000);
        builder.Property(revision => revision.Category).HasConversion<string>().HasMaxLength(64).IsRequired();
        builder.Property(revision => revision.SourceUrl).HasMaxLength(2048);
        builder.HasIndex(revision => new { revision.RecipeId, revision.RevisionNumber }).IsUnique();

        builder.HasMany(revision => revision.Ingredients)
            .WithOne()
            .HasForeignKey(ingredient => ingredient.RecipeRevisionId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(revision => revision.Steps)
            .WithOne()
            .HasForeignKey(step => step.RecipeRevisionId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(revision => revision.Tags)
            .WithOne()
            .HasForeignKey(tag => tag.RecipeRevisionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
