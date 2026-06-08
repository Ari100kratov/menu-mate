using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MenuMate.Modules.Products.Infrastructure.Database;

internal sealed class ProductRecordConfiguration : IEntityTypeConfiguration<ProductRecord>
{
    public void Configure(EntityTypeBuilder<ProductRecord> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("products");
        builder.HasKey(product => product.Id);
        builder.Property(product => product.Id).ValueGeneratedNever();
        builder.Property(product => product.Name).HasMaxLength(200).IsRequired();
        builder.Property(product => product.NormalizedName).HasMaxLength(200).IsRequired();
        builder.Property(product => product.Category).HasMaxLength(64).IsRequired();
        builder.HasIndex(product => product.NormalizedName).IsUnique();
        builder.HasIndex(product => product.Category);
    }
}
