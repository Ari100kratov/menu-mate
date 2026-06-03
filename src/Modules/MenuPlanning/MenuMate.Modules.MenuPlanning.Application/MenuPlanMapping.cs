using MenuMate.Contracts.MenuPlanning;
using MenuMate.Modules.MenuPlanning.Domain.Models;

namespace MenuMate.Modules.MenuPlanning.Application;

internal static class MenuPlanMapping
{
    public static MenuPlanResponse ToResponse(MenuPlan menuPlan) =>
        new(
            menuPlan.Id,
            menuPlan.Name,
            menuPlan.DateRange.StartDate,
            menuPlan.DateRange.EndDate,
            menuPlan.Items
                .OrderBy(item => item.Date)
                .ThenBy(item => item.MealType)
                .Select(ToResponse)
                .ToArray());

    private static MenuPlanItemResponse ToResponse(MenuPlanItem item) =>
        new(
            item.Id,
            item.Date,
            item.MealType.ToString(),
            item.RecipeId?.Value,
            item.RecipeTitle,
            item.Text,
            item.Servings.Value,
            item.Comment);
}
