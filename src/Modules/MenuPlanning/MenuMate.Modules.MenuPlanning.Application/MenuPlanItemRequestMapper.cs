using MenuMate.Contracts.MenuPlanning;
using MenuMate.Modules.MenuPlanning.Domain.Enums;
using MenuMate.Modules.MenuPlanning.Domain.Models;
using MenuMate.Modules.MenuPlanning.Domain.ValueObjects;
using MenuMate.SharedKernel;
using MenuMate.SharedKernel.Identifiers;

namespace MenuMate.Modules.MenuPlanning.Application;

internal static class MenuPlanItemRequestMapper
{
    public static Result<MenuPlanItem> Map(Guid itemId, CreateMenuPlanItemRequest request)
    {
        Result<MealType> mealType = ParseMealType(request.MealType);
        if (mealType.IsFailure)
        {
            return Result.Failure<MenuPlanItem>(mealType.Error);
        }

        Result<MenuServings> servings = MenuServings.Create(request.Servings);
        if (servings.IsFailure)
        {
            return Result.Failure<MenuPlanItem>(servings.Error);
        }

        return CreateItem(
            itemId,
            request.Date,
            mealType.Value,
            request.RecipeId,
            request.RecipeTitle,
            request.Text,
            servings.Value,
            request.Comment);
    }

    public static Result<MenuPlanItem> Map(Guid itemId, UpdateMenuPlanItemRequest request)
    {
        Result<MealType> mealType = ParseMealType(request.MealType);
        if (mealType.IsFailure)
        {
            return Result.Failure<MenuPlanItem>(mealType.Error);
        }

        Result<MenuServings> servings = MenuServings.Create(request.Servings);
        if (servings.IsFailure)
        {
            return Result.Failure<MenuPlanItem>(servings.Error);
        }

        return CreateItem(
            itemId,
            request.Date,
            mealType.Value,
            request.RecipeId,
            request.RecipeTitle,
            request.Text,
            servings.Value,
            request.Comment);
    }

    private static Result<MealType> ParseMealType(string value) =>
        Enum.TryParse(value, ignoreCase: true, out MealType mealType)
            ? mealType
            : Result.Failure<MealType>(MenuPlanningApplicationErrors.InvalidMealType);

    private static Result<MenuPlanItem> CreateItem(
        Guid itemId,
        DateOnly date,
        MealType mealType,
        Guid? recipeId,
        string? recipeTitle,
        string? text,
        MenuServings servings,
        string? comment)
    {
        if (recipeId.HasValue)
        {
            return MenuPlanItem.ForRecipe(
                itemId,
                date,
                mealType,
                RecipeId.From(recipeId.Value),
                servings,
                comment,
                recipeTitle);
        }

        if (!string.IsNullOrWhiteSpace(text))
        {
            return MenuPlanItem.ForText(
                itemId,
                date,
                mealType,
                text,
                servings,
                comment);
        }

        return Result.Failure<MenuPlanItem>(MenuPlanningApplicationErrors.InvalidItemPayload);
    }
}
