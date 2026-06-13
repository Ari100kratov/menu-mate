using MenuMate.SharedKernel;

namespace MenuMate.Modules.ShoppingLists.Domain.Errors;

/// <summary>
/// Ошибки домена списков покупок.
/// </summary>
public static class ShoppingListErrors
{
    /// <summary>
    /// Название позиции не заполнено.
    /// </summary>
    public static readonly AppError EmptyItemName = AppError.Validation(
        "ShoppingLists.EmptyItemName",
        "Название позиции списка покупок обязательно.");

    /// <summary>
    /// Для позиции с числовым количеством не указано значение.
    /// </summary>
    public static readonly AppError AmountRequired = AppError.Validation(
        "ShoppingLists.AmountRequired",
        "Укажите количество позиции или выберите «по вкусу».");

    /// <summary>
    /// Количество позиции должно быть положительным.
    /// </summary>
    public static readonly AppError AmountMustBePositive = AppError.Validation(
        "ShoppingLists.AmountMustBePositive",
        "Количество позиции списка покупок должно быть больше нуля.");
}
