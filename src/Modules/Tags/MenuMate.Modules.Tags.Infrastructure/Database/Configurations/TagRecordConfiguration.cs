using MenuMate.Modules.Tags.Infrastructure.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MenuMate.Modules.Tags.Infrastructure.Database.Configurations;

internal sealed class TagRecordConfiguration : IEntityTypeConfiguration<TagRecord>
{
    public void Configure(EntityTypeBuilder<TagRecord> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("tags");
        builder.HasKey(tag => tag.Id);
        builder.Property(tag => tag.Id).ValueGeneratedNever();
        builder.Property(tag => tag.Name).HasMaxLength(64).IsRequired();
        builder.Property(tag => tag.NormalizedName).HasMaxLength(64).IsRequired();
        builder.Property(tag => tag.Kind).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(tag => tag.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.HasIndex(tag => tag.NormalizedName).IsUnique();
        builder.HasIndex(
                [nameof(TagRecord.NormalizedName)],
                "ix_tags_normalized_name_trgm")
            .HasDatabaseName("ix_tags_normalized_name_trgm")
            .HasMethod("gin")
            .HasOperators("gin_trgm_ops");
        builder.HasIndex(tag => tag.Status);
    }
}
