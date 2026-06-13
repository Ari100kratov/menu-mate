using MenuMate.Modules.ShoppingLists.Infrastructure.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MenuMate.Modules.ShoppingLists.Infrastructure.Database.Configurations;

internal sealed class ShoppingListItemRecordConfiguration : IEntityTypeConfiguration<ShoppingListItemRecord>
{
    public void Configure(EntityTypeBuilder<ShoppingListItemRecord> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("shopping_list_items");
        builder.HasKey(item => item.Id);
        builder.Property(item => item.Id).ValueGeneratedNever();
        builder.Property(item => item.ProductId).IsRequired();
        builder.Property(item => item.Name).HasMaxLength(200).IsRequired();
        builder.Property(item => item.NormalizedName).HasMaxLength(200).IsRequired();
        builder.Property(item => item.Unit).HasConversion<string>().HasMaxLength(64).IsRequired();
        builder.Property(item => item.Category).HasConversion<string>().HasMaxLength(64).IsRequired();
        builder.Property(item => item.Comment).HasMaxLength(500);
        builder.HasIndex(item => new { item.ShoppingListId, item.NormalizedName });
        builder.HasIndex(item => item.ProductId);
        builder.HasIndex(item => item.IsPurchased);
    }
}
