using MenuMate.Modules.MenuPlanning.Infrastructure.Database.Entities;
using MenuMate.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MenuMate.Modules.MenuPlanning.Infrastructure.Database.Configurations;

internal sealed class MealSlotRecordConfiguration : IEntityTypeConfiguration<MealSlotRecord>
{
    public void Configure(EntityTypeBuilder<MealSlotRecord> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("meal_slots");
        builder.HasKey(mealSlot => mealSlot.Id);
        builder.Property(mealSlot => mealSlot.Id).ValueGeneratedNever();
        builder.Property(mealSlot => mealSlot.OwnerUserId)
            .HasConversion(userId => userId.Value, value => UserId.From(value))
            .IsRequired();
        builder.Property(mealSlot => mealSlot.Name).HasMaxLength(60).IsRequired();
        builder.HasIndex(mealSlot => mealSlot.OwnerUserId);
        builder.HasIndex(mealSlot => new { mealSlot.OwnerUserId, mealSlot.SortOrder });

    }
}
