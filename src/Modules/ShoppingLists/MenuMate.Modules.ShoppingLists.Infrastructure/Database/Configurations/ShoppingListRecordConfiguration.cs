using MenuMate.Modules.ShoppingLists.Infrastructure.Database.Entities;
using MenuMate.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MenuMate.Modules.ShoppingLists.Infrastructure.Database.Configurations;

internal sealed class ShoppingListRecordConfiguration : IEntityTypeConfiguration<ShoppingListRecord>
{
    public void Configure(EntityTypeBuilder<ShoppingListRecord> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("shopping_lists");
        builder.HasKey(shoppingList => shoppingList.Id);
        builder.Property(shoppingList => shoppingList.Id).ValueGeneratedNever();
        builder.Property(shoppingList => shoppingList.OwnerUserId)
            .HasConversion(userId => userId.Value, value => UserId.From(value))
            .IsRequired();
        builder.HasIndex(shoppingList => shoppingList.OwnerUserId).IsUnique();
        builder.HasIndex(shoppingList => new { shoppingList.OwnerUserId, shoppingList.SourceStartDate, shoppingList.SourceEndDate });
        builder.HasIndex(shoppingList => shoppingList.CreatedAt);

        builder.HasMany(shoppingList => shoppingList.Items)
            .WithOne()
            .HasForeignKey(item => item.ShoppingListId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
