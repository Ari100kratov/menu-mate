using MenuMate.Modules.RecipeImports.Domain.Enums;
using MenuMate.Modules.RecipeImports.Infrastructure.Database.Entities;
using MenuMate.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MenuMate.Modules.RecipeImports.Infrastructure.Database.Configurations;

internal sealed class RecipeImportDraftRecordConfiguration : IEntityTypeConfiguration<RecipeImportDraftRecord>
{
    public void Configure(EntityTypeBuilder<RecipeImportDraftRecord> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("recipe_import_drafts");
        builder.HasKey(draft => draft.Id);
        builder.Property(draft => draft.Id)
            .HasConversion(id => id.Value, value => ImportDraftId.From(value))
            .ValueGeneratedNever();
        builder.Property(draft => draft.OwnerUserId)
            .HasConversion(id => id.Value, value => UserId.From(value))
            .IsRequired();
        builder.Property(draft => draft.TargetRecipeId)
            .HasConversion(id => id.Value, value => RecipeId.From(value))
            .IsRequired();
        builder.Property(draft => draft.CreatedRecipeId)
            .HasConversion(
                id => id.HasValue ? id.Value.Value : (Guid?)null,
                value => value.HasValue ? RecipeId.From(value.Value) : null);
        builder.Property(draft => draft.Status)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();
        builder.Property(draft => draft.Title).HasMaxLength(240).IsRequired();
        builder.Property(draft => draft.RecipeJson).HasColumnType("jsonb").IsRequired();
        builder.Property(draft => draft.EvidenceJson).HasColumnType("jsonb").IsRequired();
        builder.Property(draft => draft.BucketName).HasMaxLength(128).IsRequired();
        builder.Property(draft => draft.ObjectKey).HasMaxLength(1024).IsRequired();
        builder.Property(draft => draft.ContentType).HasMaxLength(128).IsRequired();
        builder.Property(draft => draft.FileName).HasMaxLength(512).IsRequired();
        builder.Property(draft => draft.AdditionalSourceImagesJson)
            .HasColumnType("jsonb")
            .HasDefaultValue("[]")
            .IsRequired();
        builder.HasIndex(draft => new { draft.OwnerUserId, draft.UpdatedAt });
        builder.HasIndex(draft => draft.TargetRecipeId).IsUnique();
        builder.HasIndex(draft => draft.CreatedRecipeId)
            .IsUnique()
            .HasFilter("created_recipe_id IS NOT NULL");
    }
}
