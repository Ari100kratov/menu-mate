using MenuMate.Modules.MenuPlanning.Domain.Enums;
using MenuMate.Modules.MenuPlanning.Domain.Errors;
using MenuMate.Modules.MenuPlanning.Domain.Models;
using MenuMate.Modules.MenuPlanning.Domain.ValueObjects;
using MenuMate.SharedKernel;
using MenuMate.SharedKernel.Identifiers;

namespace MenuMate.Modules.MenuPlanning.Domain.UnitTests;

public sealed class MenuPlanTests
{
    private static readonly DateTimeOffset FixedNow = new(2026, 5, 30, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void ForTextShouldCreateNonRecipeItem()
    {
        MenuPlanItem item = MenuPlanItem.ForText(
            Guid.CreateVersion7(),
            new DateOnly(2026, 6, 1),
            MealType.Breakfast,
            "Yogurt and coffee",
            MenuServings.Create(1).Value).Value;

        Assert.False(item.IsRecipeItem);
        Assert.Null(item.RecipeId);
        Assert.Equal("Yogurt and coffee", item.Text);
    }

    [Fact]
    public void AddItemShouldRejectDateOutsidePlanRange()
    {
        MenuPlanDateRange range = MenuPlanDateRange.Create(
            new DateOnly(2026, 6, 1),
            new DateOnly(2026, 6, 7)).Value;
        MenuPlan plan = MenuPlan.Create(Guid.CreateVersion7(), UserId.Create(), "Week", range, FixedNow).Value;
        var item = MenuPlanItem.ForRecipe(
            Guid.CreateVersion7(),
            new DateOnly(2026, 6, 8),
            MealType.Dinner,
            RecipeId.Create(),
            RecipeRevisionId.Create(),
            MenuServings.Create(2).Value);

        Result result = plan.AddItem(item, FixedNow);

        Assert.True(result.IsFailure);
        Assert.Equal(MenuPlanErrors.DateOutsidePlanRange, result.Error);
        Assert.Empty(plan.Items);
    }

    [Fact]
    public void UpdateItemShouldReplaceExistingItem()
    {
        MenuPlanDateRange range = MenuPlanDateRange.Create(
            new DateOnly(2026, 6, 1),
            new DateOnly(2026, 6, 7)).Value;
        MenuPlan plan = MenuPlan.Create(Guid.CreateVersion7(), UserId.Create(), "Week", range, FixedNow).Value;
        var itemId = Guid.CreateVersion7();
        MenuPlanItem item = MenuPlanItem.ForText(
            itemId,
            new DateOnly(2026, 6, 1),
            MealType.Breakfast,
            "Yogurt",
            MenuServings.Create(1).Value).Value;
        MenuPlanItem updatedItem = MenuPlanItem.ForText(
            itemId,
            new DateOnly(2026, 6, 2),
            MealType.Lunch,
            "Soup",
            MenuServings.Create(2).Value).Value;

        plan.AddItem(item, FixedNow);
        Result result = plan.UpdateItem(updatedItem, FixedNow.AddMinutes(5));

        Assert.True(result.IsSuccess);
        MenuPlanItem savedItem = Assert.Single(plan.Items);
        Assert.Equal(itemId, savedItem.Id);
        Assert.Equal(new DateOnly(2026, 6, 2), savedItem.Date);
        Assert.Equal(MealType.Lunch, savedItem.MealType);
        Assert.Equal("Soup", savedItem.Text);
    }
}
