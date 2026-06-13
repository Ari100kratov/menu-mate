#pragma warning disable CS1573

namespace MenuMate.Contracts.MenuPlanning;

/// <summary>
/// Календарь меню за выбранный диапазон дат.
/// </summary>
/// <param name="StartDate">Дата начала выбранного диапазона.</param>
/// <param name="EndDate">Дата окончания выбранного диапазона.</param>
/// <param name="MealSlots">Приемы пищи пользователя.</param>
/// <param name="Items">Позиции меню в выбранном диапазоне.</param>
public sealed record MenuCalendarResponse(
    DateOnly StartDate,
    DateOnly EndDate,
    IReadOnlyCollection<MealSlotResponse> MealSlots,
    IReadOnlyCollection<MenuCalendarItemResponse> Items);

/// <summary>
/// Настраиваемый прием пищи пользователя.
/// </summary>
/// <param name="Id">Идентификатор приема пищи.</param>
/// <param name="Name">Название, видимое пользователю.</param>
/// <param name="SortOrder">Порядок отображения.</param>
public sealed record MealSlotResponse(Guid Id, string Name, int SortOrder);

/// <summary>
/// Позиция календаря меню.
/// </summary>
/// <param name="Id">Идентификатор позиции меню.</param>
/// <param name="Date">Дата приема пищи.</param>
/// <param name="MealSlotId">Идентификатор приема пищи.</param>
/// <param name="Position">Порядок внутри приема пищи.</param>
/// <param name="RecipeId">Идентификатор рецепта для позиции на основе рецепта.</param>
/// <param name="RecipeRevisionId">Идентификатор immutable revision рецепта.</param>
/// <param name="RecipeTitle">Снимок названия рецепта.</param>
/// <param name="Text">Свободный текст для позиции без рецепта.</param>
/// <param name="Servings">Количество порций.</param>
/// <param name="Comment">Необязательный комментарий.</param>
public sealed record MenuCalendarItemResponse(
    Guid Id,
    DateOnly Date,
    Guid MealSlotId,
    int Position,
    Guid? RecipeId,
    Guid? RecipeRevisionId,
    string? RecipeTitle,
    string? Text,
    int Servings,
    string? Comment,
    Uri? ImageUrl);

/// <summary>
/// Запрос на добавление позиции меню.
/// </summary>
/// <param name="Date">Дата приема пищи.</param>
/// <param name="MealSlotId">Идентификатор приема пищи.</param>
/// <param name="RecipeId">Идентификатор рецепта для позиции на основе рецепта.</param>
/// <param name="RecipeRevisionId">Идентификатор immutable revision рецепта.</param>
/// <param name="RecipeTitle">Снимок названия рецепта.</param>
/// <param name="Text">Свободный текст для позиции без рецепта.</param>
/// <param name="Servings">Количество порций.</param>
/// <param name="Comment">Необязательный комментарий.</param>
public sealed record CreateMenuCalendarItemRequest(
    DateOnly Date,
    Guid MealSlotId,
    Guid? RecipeId,
    Guid? RecipeRevisionId,
    string? RecipeTitle,
    string? Text,
    int Servings,
    string? Comment);

/// <summary>
/// Запрос на обновление позиции меню.
/// </summary>
/// <param name="Date">Дата приема пищи.</param>
/// <param name="MealSlotId">Идентификатор приема пищи.</param>
/// <param name="RecipeId">Идентификатор рецепта для позиции на основе рецепта.</param>
/// <param name="RecipeRevisionId">Идентификатор immutable revision рецепта.</param>
/// <param name="RecipeTitle">Снимок названия рецепта.</param>
/// <param name="Text">Свободный текст для позиции без рецепта.</param>
/// <param name="Servings">Количество порций.</param>
/// <param name="Comment">Необязательный комментарий.</param>
public sealed record UpdateMenuCalendarItemRequest(
    DateOnly Date,
    Guid MealSlotId,
    Guid? RecipeId,
    Guid? RecipeRevisionId,
    string? RecipeTitle,
    string? Text,
    int Servings,
    string? Comment);

/// <summary>
/// Запрос на создание приема пищи.
/// </summary>
/// <param name="Name">Название приема пищи.</param>
public sealed record CreateMealSlotRequest(string Name);

/// <summary>
/// Запрос на переименование приема пищи.
/// </summary>
/// <param name="Name">Название приема пищи.</param>
public sealed record UpdateMealSlotRequest(string Name);

/// <summary>
/// Запрос на изменение порядка приемов пищи.
/// </summary>
/// <param name="MealSlotIds">Идентификаторы приемов пищи в новом порядке.</param>
public sealed record ReorderMealSlotsRequest(IReadOnlyCollection<Guid> MealSlotIds);
