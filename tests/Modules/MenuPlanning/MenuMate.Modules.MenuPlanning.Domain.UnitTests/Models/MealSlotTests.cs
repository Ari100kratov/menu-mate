using MenuMate.Modules.MenuPlanning.Domain.Errors;
using MenuMate.Modules.MenuPlanning.Domain.Models;
using MenuMate.SharedKernel;
using MenuMate.SharedKernel.Identifiers;

namespace MenuMate.Modules.MenuPlanning.Domain.UnitTests.Models;

public sealed class MealSlotTests
{
    private static readonly DateTimeOffset FixedNow = new(2026, 6, 12, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void CreateShouldTrimNameAndKeepOwner()
    {
        var ownerUserId = UserId.Create();

        MealSlot slot = MealSlot.Create(
            Guid.CreateVersion7(),
            ownerUserId,
            "  Завтрак  ",
            0,
            FixedNow).Value;

        Assert.Equal(ownerUserId, slot.OwnerUserId);
        Assert.Equal("Завтрак", slot.Name);
        Assert.Equal(0, slot.SortOrder);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void CreateShouldRejectEmptyName(string name)
    {
        Result<MealSlot> result = MealSlot.Create(Guid.CreateVersion7(), UserId.Create(), name, 0, FixedNow);

        Assert.True(result.IsFailure);
        Assert.Equal(MenuCalendarErrors.EmptyMealSlotName, result.Error);
    }

    [Fact]
    public void RenameShouldRejectNameLongerThanLimitWithoutChangingSlot()
    {
        MealSlot slot = MealSlot.Create(
            Guid.CreateVersion7(),
            UserId.Create(),
            "Завтрак",
            0,
            FixedNow).Value;

        Result result = slot.Rename(new string('а', MealSlot.MaxNameLength + 1), FixedNow.AddMinutes(1));

        Assert.True(result.IsFailure);
        Assert.Equal(MenuCalendarErrors.MealSlotNameTooLong, result.Error);
        Assert.Equal("Завтрак", slot.Name);
        Assert.Equal(FixedNow, slot.UpdatedAt);
    }

    [Fact]
    public void RenameAndChangeSortOrderShouldUpdateSlot()
    {
        MealSlot slot = MealSlot.Create(
            Guid.CreateVersion7(),
            UserId.Create(),
            "Завтрак",
            0,
            FixedNow).Value;
        DateTimeOffset updatedAt = FixedNow.AddMinutes(1);

        Assert.True(slot.Rename("Поздний завтрак", updatedAt).IsSuccess);
        slot.ChangeSortOrder(2, updatedAt);

        Assert.Equal("Поздний завтрак", slot.Name);
        Assert.Equal(2, slot.SortOrder);
        Assert.Equal(updatedAt, slot.UpdatedAt);
    }
}
