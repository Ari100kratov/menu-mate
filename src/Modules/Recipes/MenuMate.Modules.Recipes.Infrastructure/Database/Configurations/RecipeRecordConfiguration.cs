using MenuMate.Modules.Recipes.Infrastructure.Database.Entities;
using MenuMate.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace MenuMate.Modules.Recipes.Infrastructure.Database.Configurations;

internal sealed class RecipeRecordConfiguration : IEntityTypeConfiguration<RecipeRecord>
{
    public void Configure(EntityTypeBuilder<RecipeRecord> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var nullableRevisionIdConverter = new ValueConverter<RecipeRevisionId?, Guid?>(
            revisionId => revisionId.HasValue ? revisionId.Value.Value : null,
            value => value.HasValue ? RecipeRevisionId.From(value.Value) : null);

        builder.ToTable("recipes");
        builder.HasKey(recipe => recipe.Id);
        builder.Property(recipe => recipe.Id).ValueGeneratedNever();
        builder.Property(recipe => recipe.OwnerUserId)
            .HasConversion(userId => userId.Value, value => UserId.From(value))
            .IsRequired();
        builder.Property(recipe => recipe.Title).HasMaxLength(160).IsRequired();
        builder.Property(recipe => recipe.Description).HasMaxLength(2000);
        builder.Property(recipe => recipe.Category).HasConversion<string>().HasMaxLength(64).IsRequired();
        builder.Property(recipe => recipe.Visibility).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(recipe => recipe.CurrentRevisionId)
            .HasConversion(revisionId => revisionId.Value, value => RecipeRevisionId.From(value))
            .IsRequired();
        builder.Property(recipe => recipe.SourceRevisionId).HasConversion(nullableRevisionIdConverter);
        builder.Property(recipe => recipe.SourceUrl).HasMaxLength(2048);
        builder.HasIndex(recipe => recipe.Category);
        builder.HasIndex(recipe => recipe.Title);
        builder.HasIndex(recipe => recipe.Visibility);
        builder.HasIndex(recipe => recipe.CurrentRevisionId).IsUnique();
        builder.HasIndex(recipe => recipe.SourceRecipeId);
        builder.HasIndex(recipe => recipe.IsDeleted);
        builder.HasIndex(recipe => recipe.OwnerUserId);

        builder.HasMany(recipe => recipe.Ingredients)
            .WithOne()
            .HasForeignKey(ingredient => ingredient.RecipeId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(recipe => recipe.Steps)
            .WithOne()
            .HasForeignKey(step => step.RecipeId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(recipe => recipe.Tags)
            .WithOne()
            .HasForeignKey(tag => tag.RecipeId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(recipe => recipe.Images)
            .WithOne()
            .HasForeignKey(image => image.RecipeId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(recipe => recipe.Revisions)
            .WithOne()
            .HasForeignKey(revision => revision.RecipeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
