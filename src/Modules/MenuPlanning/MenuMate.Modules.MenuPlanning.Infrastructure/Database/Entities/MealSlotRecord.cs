using MenuMate.Modules.MenuPlanning.Domain.Models;
using MenuMate.SharedKernel.Identifiers;

namespace MenuMate.Modules.MenuPlanning.Infrastructure.Database.Entities;

internal sealed class MealSlotRecord
{
    public Guid Id { get; set; }

    public UserId OwnerUserId { get; set; }

    public string Name { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public static MealSlotRecord FromDomain(MealSlot mealSlot)
    {
        var record = new MealSlotRecord();
        record.Apply(mealSlot);
        return record;
    }

    public void Apply(MealSlot mealSlot)
    {
        Id = mealSlot.Id;
        OwnerUserId = mealSlot.OwnerUserId;
        Name = mealSlot.Name;
        SortOrder = mealSlot.SortOrder;
        CreatedAt = mealSlot.CreatedAt;
        UpdatedAt = mealSlot.UpdatedAt;
    }

    public MealSlot ToDomain() =>
        MealSlot.Rehydrate(
            Id,
            OwnerUserId,
            Name,
            SortOrder,
            CreatedAt,
            UpdatedAt);
}
