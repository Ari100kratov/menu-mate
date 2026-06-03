using MenuMate.SharedKernel;

namespace MenuMate.Modules.MenuPlanning.Domain.Errors;

/// <summary>
/// Ошибки домена планирования меню.
/// </summary>
public static class MenuPlanErrors
{
    /// <summary>
    /// Название плана пустое.
    /// </summary>
    public static readonly AppError EmptyName = AppError.Validation(
        "MenuPlanning.EmptyName",
        "Название меню не может быть пустым.");

    /// <summary>
    /// Диапазон дат некорректен.
    /// </summary>
    public static readonly AppError InvalidDateRange = AppError.Validation(
        "MenuPlanning.InvalidDateRange",
        "Дата окончания меню не может быть раньше даты начала.");

    /// <summary>
    /// Количество персон некорректно.
    /// </summary>
    public static readonly AppError InvalidServings = AppError.Validation(
        "MenuPlanning.InvalidServings",
        "Количество персон должно быть от 1 до 100.");

    /// <summary>
    /// Произвольная позиция меню пустая.
    /// </summary>
    public static readonly AppError EmptyTextItem = AppError.Validation(
        "MenuPlanning.EmptyTextItem",
        "Произвольная позиция меню не может быть пустой.");

    /// <summary>
    /// Позиция меню не найдена.
    /// </summary>
    public static readonly AppError ItemNotFound = AppError.NotFound(
        "MenuPlanning.ItemNotFound",
        "Позиция меню не найдена.");

    /// <summary>
    /// Дата позиции выходит за диапазон меню.
    /// </summary>
    public static readonly AppError DateOutsidePlanRange = AppError.Validation(
        "MenuPlanning.DateOutsidePlanRange",
        "Дата позиции меню должна входить в диапазон плана.");

    /// <summary>
    /// В плане есть позиции за пределами нового диапазона дат.
    /// </summary>
    public static readonly AppError ItemsOutsidePlanRange = AppError.Validation(
        "MenuPlanning.ItemsOutsidePlanRange",
        "Новый диапазон дат должен включать все существующие позиции плана меню.");
}


