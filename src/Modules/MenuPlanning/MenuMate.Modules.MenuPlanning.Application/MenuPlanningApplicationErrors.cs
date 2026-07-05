using MenuMate.SharedKernel;

namespace MenuMate.Modules.MenuPlanning.Application;

internal static class MenuPlanningApplicationErrors
{
    public static readonly AppError AccessDenied = AppError.Forbidden(
        "MenuPlanning.AccessDenied",
        "Календарь меню принадлежит другому пользователю.");

    public static readonly AppError DuplicateMealSlotName = AppError.Conflict(
        "MenuPlanning.DuplicateMealSlotName",
        "Прием пищи с таким названием уже существует.");

    public static readonly AppError InvalidMealSlotOrder = AppError.Validation(
        "MenuPlanning.InvalidMealSlotOrder",
        "Передайте все приемы пищи ровно по одному разу.");

    public static AppError MealSlotNotFound(Guid mealSlotId) => AppError.NotFound(
        "MenuPlanning.MealSlotNotFound",
        $"Прием пищи с идентификатором '{mealSlotId}' не найден.");

    public static AppError ItemNotFound(Guid itemId) => AppError.NotFound(
        "MenuPlanning.ItemNotFound",
        $"Позиция меню с идентификатором '{itemId}' не найдена.");
}
