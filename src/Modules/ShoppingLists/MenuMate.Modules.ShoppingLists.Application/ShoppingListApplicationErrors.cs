using MenuMate.SharedKernel;

namespace MenuMate.Modules.ShoppingLists.Application;

internal static class ShoppingListApplicationErrors
{
    public static readonly AppError AccessDenied = AppError.Forbidden(
        "ShoppingLists.AccessDenied",
        "Список покупок принадлежит другому пользователю.");

    public static readonly AppError InvalidUnit = AppError.Validation(
        "ShoppingLists.InvalidUnit",
        "Единица измерения указана в неизвестном формате.");

    public static readonly AppError InvalidQuantityKind = AppError.Validation(
        "ShoppingLists.InvalidQuantityKind",
        "Тип количества должен быть Exact, Approximate или ToTaste.");

    public static readonly AppError InvalidProductCategory = AppError.Validation(
        "ShoppingLists.InvalidProductCategory",
        "Категория продукта указана в неизвестном формате.");

    public static AppError NotFound(Guid shoppingListId) => AppError.NotFound(
        "ShoppingLists.NotFound",
        $"Список покупок с идентификатором '{shoppingListId}' не найден.");

    public static AppError MenuPlanNotFound(Guid menuPlanId) => AppError.NotFound(
        "ShoppingLists.MenuPlanNotFound",
        $"План меню с идентификатором '{menuPlanId}' не найден.");

    public static AppError ItemNotFound(Guid itemId) => AppError.NotFound(
        "ShoppingLists.ItemNotFound",
        $"Позиция списка покупок с идентификатором '{itemId}' не найдена.");
}
