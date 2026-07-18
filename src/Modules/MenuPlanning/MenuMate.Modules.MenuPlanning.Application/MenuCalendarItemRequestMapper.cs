using MenuMate.Contracts.MenuPlanning;
using MenuMate.Modules.MenuPlanning.Domain.Errors;
using MenuMate.Modules.MenuPlanning.Domain.Models;
using MenuMate.Modules.MenuPlanning.Domain.ValueObjects;
using MenuMate.SharedKernel;
using MenuMate.SharedKernel.Identifiers;

namespace MenuMate.Modules.MenuPlanning.Application;

internal static class MenuCalendarItemRequestMapper
{
    public static Result<MenuCalendarItem> Map(
        Guid itemId,
        UserId ownerUserId,
        int position,
        CreateMenuCalendarItemRequest request,
        string? recipeTitle,
        DateTimeOffset now) =>
        CreateItem(
            itemId,
            ownerUserId,
            request.Date,
            request.MealSlotId,
            position,
            request.RecipeId,
            request.RecipeRevisionId,
            recipeTitle,
            request.Text,
            request.Servings,
            request.Comment,
            now);

    private static Result<MenuCalendarItem> CreateItem(
        Guid itemId,
        UserId ownerUserId,
        DateOnly date,
        Guid mealSlotId,
        int position,
        Guid? recipeId,
        Guid? recipeRevisionId,
        string? recipeTitle,
        string? text,
        int servingsValue,
        string? comment,
        DateTimeOffset now)
    {
        Result<MenuServings> servings = MenuServings.Create(servingsValue);
        if (servings.IsFailure)
        {
            return Result.Failure<MenuCalendarItem>(servings.Error);
        }

        if (recipeId.HasValue)
        {
            if (!recipeRevisionId.HasValue)
            {
                return Result.Failure<MenuCalendarItem>(MenuCalendarErrors.InvalidItemPayload);
            }

            return MenuCalendarItem.ForRecipe(
                itemId,
                ownerUserId,
                date,
                mealSlotId,
                position,
                RecipeId.From(recipeId.Value),
                RecipeRevisionId.From(recipeRevisionId.Value),
                servings.Value,
                now,
                recipeTitle,
                comment);
        }

        if (!string.IsNullOrWhiteSpace(text))
        {
            return MenuCalendarItem.ForText(
                itemId,
                ownerUserId,
                date,
                mealSlotId,
                position,
                text,
                servings.Value,
                now,
                comment);
        }

        return Result.Failure<MenuCalendarItem>(MenuCalendarErrors.InvalidItemPayload);
    }
}
