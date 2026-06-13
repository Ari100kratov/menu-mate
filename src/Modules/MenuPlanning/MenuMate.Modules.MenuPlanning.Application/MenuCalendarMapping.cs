using MenuMate.Contracts.MenuPlanning;
using MenuMate.Modules.MenuPlanning.Domain.Models;

namespace MenuMate.Modules.MenuPlanning.Application;

internal static class MenuCalendarMapping
{
    public static MenuCalendarItemResponse ToResponse(MenuCalendarItem item) =>
        new(
            item.Id,
            item.Date,
            item.MealSlotId,
            item.Position,
            item.RecipeId?.Value,
            item.RecipeRevisionId?.Value,
            item.RecipeTitle,
            item.Text,
            item.Servings.Value,
            item.Comment,
            null);
}
