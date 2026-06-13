using MenuMate.SharedKernel;

namespace MenuMate.Modules.MenuPlanning.Domain.Errors;

/// <summary>
/// Ошибки домена календаря меню.
/// </summary>
public static class MenuCalendarErrors
{
    /// <summary>Название приема пищи пустое.</summary>
    public static readonly AppError EmptyMealSlotName = AppError.Validation(
        "MenuPlanning.EmptyMealSlotName",
        "Название приема пищи не может быть пустым.");

    /// <summary>Название приема пищи слишком длинное.</summary>
    public static readonly AppError MealSlotNameTooLong = AppError.Validation(
        "MenuPlanning.MealSlotNameTooLong",
        "Название приема пищи должно быть не длиннее 60 символов.");

    /// <summary>Диапазон дат некорректен.</summary>
    public static readonly AppError InvalidDateRange = AppError.Validation(
        "MenuPlanning.InvalidDateRange",
        "Дата окончания меню не может быть раньше даты начала.");

    /// <summary>Диапазон дат слишком большой.</summary>
    public static readonly AppError DateRangeTooLong = AppError.Validation(
        "MenuPlanning.DateRangeTooLong",
        "Диапазон меню не должен превышать 366 дней.");

    /// <summary>Количество порций некорректно.</summary>
    public static readonly AppError InvalidServings = AppError.Validation(
        "MenuPlanning.InvalidServings",
        "Количество персон должно быть от 1 до 100.");

    /// <summary>Произвольная позиция меню пустая.</summary>
    public static readonly AppError EmptyTextItem = AppError.Validation(
        "MenuPlanning.EmptyTextItem",
        "Произвольная позиция меню не может быть пустой.");

    /// <summary>Позиция меню не содержит рецепт или текст.</summary>
    public static readonly AppError InvalidItemPayload = AppError.Validation(
        "MenuPlanning.InvalidItemPayload",
        "Позиция меню должна содержать рецепт или произвольный текст.");

    /// <summary>Прием пищи не выбран.</summary>
    public static readonly AppError EmptyMealSlotId = AppError.Validation(
        "MenuPlanning.EmptyMealSlotId",
        "Выберите прием пищи.");

    /// <summary>Порядок позиции некорректен.</summary>
    public static readonly AppError InvalidPosition = AppError.Validation(
        "MenuPlanning.InvalidPosition",
        "Порядок позиции меню не может быть отрицательным.");

    /// <summary>Нельзя удалить последний прием пищи.</summary>
    public static readonly AppError CannotDeleteLastMealSlot = AppError.Validation(
        "MenuPlanning.CannotDeleteLastMealSlot",
        "В меню должен остаться хотя бы один прием пищи.");
}
