using MenuMate.SharedKernel;

namespace MenuMate.Modules.MenuPlanning.Application;

internal static class MenuPlanningApplicationErrors
{
    public static readonly AppError InvalidMealType = AppError.Validation(
        "MenuPlanning.InvalidMealType",
        "Тип приема пищи указан в неизвестном формате.");

    public static readonly AppError InvalidItemPayload = AppError.Validation(
        "MenuPlanning.InvalidItemPayload",
        "Позиция меню должна содержать рецепт или произвольный текст.");

    public static readonly AppError AccessDenied = AppError.Forbidden(
        "MenuPlanning.AccessDenied",
        "План меню принадлежит другому пользователю.");

    public static AppError NotFound(Guid menuPlanId) => AppError.NotFound(
        "MenuPlanning.NotFound",
        $"План меню с идентификатором '{menuPlanId}' не найден.");

    public static AppError ItemNotFound(Guid itemId) => AppError.NotFound(
        "MenuPlanning.ItemNotFound",
        $"Позиция плана меню с идентификатором '{itemId}' не найдена.");
}
