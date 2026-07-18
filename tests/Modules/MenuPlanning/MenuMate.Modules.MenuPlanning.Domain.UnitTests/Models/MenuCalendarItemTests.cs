using MenuMate.Modules.MenuPlanning.Domain.Errors;
using MenuMate.Modules.MenuPlanning.Domain.Models;
using MenuMate.Modules.MenuPlanning.Domain.ValueObjects;
using MenuMate.SharedKernel;
using MenuMate.SharedKernel.Identifiers;

namespace MenuMate.Modules.MenuPlanning.Domain.UnitTests.Models;

public sealed class MenuCalendarItemTests
{
    private static readonly DateTimeOffset FixedNow = new(2026, 6, 12, 12, 0, 0, TimeSpan.Zero);
    private static readonly DateOnly Date = new(2026, 6, 12);

    [Fact]
    public void ForRecipeShouldCreatePinnedRecipeItem()
    {
        var recipeId = RecipeId.Create();
        var revisionId = RecipeRevisionId.Create();

        MenuCalendarItem item = MenuCalendarItem.ForRecipe(
            Guid.CreateVersion7(),
            UserId.Create(),
            Date,
            Guid.CreateVersion7(),
            0,
            recipeId,
            revisionId,
            MenuServings.Create(2).Value,
            FixedNow,
            "  Паста  ",
            "  Без сыра  ").Value;

        Assert.True(item.IsRecipeItem);
        Assert.Equal(recipeId, item.RecipeId);
        Assert.Equal(revisionId, item.RecipeRevisionId);
        Assert.Equal("Паста", item.RecipeTitle);
        Assert.Equal("Без сыра", item.Comment);
        Assert.Null(item.Text);
    }

    [Fact]
    public void ForTextShouldTrimTextAndCreateNonRecipeItem()
    {
        MenuCalendarItem item = CreateTextItem("  Йогурт и кофе  ");

        Assert.False(item.IsRecipeItem);
        Assert.Equal("Йогурт и кофе", item.Text);
        Assert.Null(item.RecipeId);
    }

    [Theory]
    [InlineData("", 0, "empty-slot")]
    [InlineData("00000000-0000-0000-0000-000000000001", -1, "position")]
    public void ForTextShouldRejectInvalidPlacement(string mealSlotId, int position, string expectedError)
    {
        ArgumentNullException.ThrowIfNull(mealSlotId);
        ArgumentNullException.ThrowIfNull(expectedError);

        Guid slotId = mealSlotId.Length == 0 ? Guid.Empty : Guid.Parse(mealSlotId);

        Result<MenuCalendarItem> result = MenuCalendarItem.ForText(
            Guid.CreateVersion7(),
            UserId.Create(),
            Date,
            slotId,
            position,
            "Текст",
            MenuServings.Create(1).Value,
            FixedNow);

        Assert.True(result.IsFailure);
        Assert.Equal(
            expectedError == "empty-slot"
                ? MenuCalendarErrors.EmptyMealSlotId
                : MenuCalendarErrors.InvalidPosition,
            result.Error);
    }

    [Fact]
    public void ForTextShouldRejectEmptyText()
    {
        Result<MenuCalendarItem> result = MenuCalendarItem.ForText(
            Guid.CreateVersion7(),
            UserId.Create(),
            Date,
            Guid.CreateVersion7(),
            0,
            " ",
            MenuServings.Create(1).Value,
            FixedNow);

        Assert.True(result.IsFailure);
        Assert.Equal(MenuCalendarErrors.EmptyTextItem, result.Error);
    }

    [Fact]
    public void UpdateShouldPreservePinnedRecipeRevision()
    {
        var recipeId = RecipeId.Create();
        var revisionId = RecipeRevisionId.Create();
        MenuCalendarItem item = MenuCalendarItem.ForRecipe(
            Guid.CreateVersion7(),
            UserId.Create(),
            Date,
            Guid.CreateVersion7(),
            0,
            recipeId,
            revisionId,
            MenuServings.Create(2).Value,
            FixedNow,
            "Овсянка").Value;

        Result result = item.Update(
            Date,
            item.MealSlotId,
            "Подменить текстом",
            MenuServings.Create(2).Value,
            FixedNow.AddMinutes(1));

        Assert.True(result.IsSuccess);
        Assert.Equal(recipeId, item.RecipeId);
        Assert.Equal(revisionId, item.RecipeRevisionId);
        Assert.Equal("Овсянка", item.RecipeTitle);
        Assert.Null(item.Text);
    }

    [Fact]
    public void ChangePositionShouldRejectNegativeValue()
    {
        MenuCalendarItem item = CreateTextItem("Овсянка");

        Result result = item.ChangePosition(-1, FixedNow.AddMinutes(1));

        Assert.True(result.IsFailure);
        Assert.Equal(MenuCalendarErrors.InvalidPosition, result.Error);
        Assert.Equal(0, item.Position);
    }

    private static MenuCalendarItem CreateTextItem(string text) =>
        MenuCalendarItem.ForText(
            Guid.CreateVersion7(),
            UserId.Create(),
            Date,
            Guid.CreateVersion7(),
            0,
            text,
            MenuServings.Create(1).Value,
            FixedNow).Value;
}
