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

    public static readonly AppError InvalidProductCategory = AppError.Validation(
        "ShoppingLists.InvalidProductCategory",
        "Категория продукта указана в неизвестном формате.");

    public static AppError NotFound(Guid shoppingListId) => AppError.NotFound(
        "ShoppingLists.NotFound",
        $"Список покупок с идентификатором '{shoppingListId}' не найден.");

    public static readonly AppError InvalidDateRange = AppError.Validation(
        "ShoppingLists.InvalidDateRange",
        "Дата окончания списка покупок не может быть раньше даты начала.");

    public static readonly AppError InvalidServings = AppError.Validation(
        "ShoppingLists.InvalidServings",
        "Количество порций должно быть больше нуля.");

    public static readonly AppError EmptyList = AppError.NotFound(
        "ShoppingLists.Empty",
        "Список покупок пока пуст.");

    public static AppError ItemNotFound(Guid itemId) => AppError.NotFound(
        "ShoppingLists.ItemNotFound",
        $"Позиция списка покупок с идентификатором '{itemId}' не найдена.");
}
