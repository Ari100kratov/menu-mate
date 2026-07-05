using MenuMate.Modules.Recipes.Domain.Enums;
using MenuMate.Modules.Recipes.Infrastructure.Database.Entities;
using MenuMate.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MenuMate.Modules.Recipes.Infrastructure.Database.Configurations;

internal sealed class RecipeImageRecordConfiguration : IEntityTypeConfiguration<RecipeImageRecord>
{
    public void Configure(EntityTypeBuilder<RecipeImageRecord> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        const string scopeColumnName = "scope";

        builder.ToTable("recipe_images", table => table.HasCheckConstraint(
            "ck_recipe_images_step_number_matches_scope",
            $"""
            ({scopeColumnName} = '{RecipeImageScope.Step}' AND step_number IS NOT NULL)
            OR ({scopeColumnName} = '{RecipeImageScope.Cover}' AND step_number IS NULL)
            """));
        builder.HasKey(image => image.Id);
        builder.Property(image => image.Id).ValueGeneratedNever();
        builder.Property(image => image.OwnerUserId)
            .HasConversion(userId => userId.Value, value => UserId.From(value))
            .IsRequired();
        builder.Property(image => image.Scope)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();
        builder.Property(image => image.BucketName).HasMaxLength(63).IsRequired();
        builder.Property(image => image.ObjectKey).HasMaxLength(1024).IsRequired();
        builder.Property(image => image.ContentType).HasMaxLength(100).IsRequired();
        builder.Property(image => image.OriginalFileName).HasMaxLength(255);
        builder.Property(image => image.AltText).HasMaxLength(500);
        builder.Property(image => image.SourceUrl).HasMaxLength(2048);
        builder.Property(image => image.AuthorName).HasMaxLength(500);
        builder.Property(image => image.LicenseName).HasMaxLength(200);
        builder.Property(image => image.LicenseUrl).HasMaxLength(2048);
        builder.HasIndex(image => image.OwnerUserId);
        builder.HasIndex(image => image.RecipeId);
        builder.HasIndex(image => new { image.RecipeId, image.Scope });
        builder.HasIndex(image => image.ObjectKey).IsUnique();
    }
}
