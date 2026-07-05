using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MenuMate.DataImporter.Infrastructure.Database;

/// <summary>
/// Конфигурация записи состояния импорта.
/// </summary>
public sealed class ImportItemRecordConfiguration : IEntityTypeConfiguration<ImportItemRecord>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<ImportItemRecord> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("import_items");
        builder.HasKey(item => item.Id);
        builder.Property(item => item.Id).ValueGeneratedNever();
        builder.Property(item => item.Source).HasMaxLength(100).IsRequired();
        builder.Property(item => item.ExternalId).HasMaxLength(200).IsRequired();
        builder.Property(item => item.SourceUrl)
            .HasConversion(url => url.ToString(), value => new Uri(value, UriKind.Absolute))
            .HasMaxLength(2048)
            .IsRequired();
        builder.Property(item => item.ContentHash).HasMaxLength(128).IsRequired();
        builder.Property(item => item.Status).HasMaxLength(32).IsRequired();
        builder.Property(item => item.LastError).HasMaxLength(4000);
        builder.HasIndex(item => new { item.Source, item.ExternalId }).IsUnique();
        builder.HasIndex(item => item.RecipeId);
    }
}
