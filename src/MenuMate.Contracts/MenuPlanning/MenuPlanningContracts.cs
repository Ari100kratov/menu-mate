namespace MenuMate.Contracts.MenuPlanning;

/// <summary>
/// План меню, возвращаемый MenuPlanning API.
/// </summary>
/// <param name="Id">Идентификатор плана меню.</param>
/// <param name="Name">Название плана меню.</param>
/// <param name="StartDate">Дата начала.</param>
/// <param name="EndDate">Дата окончания.</param>
/// <param name="Items">Пункты плана меню.</param>
public sealed record MenuPlanResponse(
    Guid Id,
    string Name,
    DateOnly StartDate,
    DateOnly EndDate,
    IReadOnlyCollection<MenuPlanItemResponse> Items);

/// <summary>
/// Пункт плана меню, возвращаемый MenuPlanning API.
/// </summary>
/// <param name="Id">Идентификатор пункта меню.</param>
/// <param name="Date">Дата приема пищи.</param>
/// <param name="MealType">Тип приема пищи.</param>
/// <param name="RecipeId">Идентификатор рецепта для пункта на основе рецепта.</param>
/// <param name="RecipeTitle">Снимок названия рецепта.</param>
/// <param name="Text">Свободный текст для пункта без рецепта.</param>
/// <param name="Servings">Количество порций.</param>
/// <param name="Comment">Необязательный комментарий.</param>
public sealed record MenuPlanItemResponse(
    Guid Id,
    DateOnly Date,
    string MealType,
    Guid? RecipeId,
    string? RecipeTitle,
    string? Text,
    int Servings,
    string? Comment);

/// <summary>
/// Запрос на создание плана меню.
/// </summary>
/// <param name="Name">Название плана меню.</param>
/// <param name="StartDate">Дата начала.</param>
/// <param name="EndDate">Дата окончания.</param>
public sealed record CreateMenuPlanRequest(string Name, DateOnly StartDate, DateOnly EndDate);

/// <summary>
/// Запрос на обновление основных параметров плана меню.
/// </summary>
/// <param name="Name">Название плана меню.</param>
/// <param name="StartDate">Дата начала.</param>
/// <param name="EndDate">Дата окончания.</param>
public sealed record UpdateMenuPlanRequest(string Name, DateOnly StartDate, DateOnly EndDate);

/// <summary>
/// Запрос на добавление пункта меню.
/// </summary>
/// <param name="Date">Дата приема пищи.</param>
/// <param name="MealType">Тип приема пищи.</param>
/// <param name="RecipeId">Идентификатор рецепта для пункта на основе рецепта.</param>
/// <param name="RecipeTitle">Снимок названия рецепта.</param>
/// <param name="Text">Свободный текст для пункта без рецепта.</param>
/// <param name="Servings">Количество порций.</param>
/// <param name="Comment">Необязательный комментарий.</param>
public sealed record CreateMenuPlanItemRequest(
    DateOnly Date,
    string MealType,
    Guid? RecipeId,
    string? RecipeTitle,
    string? Text,
    int Servings,
    string? Comment);

/// <summary>
/// Запрос на обновление пункта меню.
/// </summary>
/// <param name="Date">Дата приема пищи.</param>
/// <param name="MealType">Тип приема пищи.</param>
/// <param name="RecipeId">Идентификатор рецепта для пункта на основе рецепта.</param>
/// <param name="RecipeTitle">Снимок названия рецепта.</param>
/// <param name="Text">Свободный текст для пункта без рецепта.</param>
/// <param name="Servings">Количество порций.</param>
/// <param name="Comment">Необязательный комментарий.</param>
public sealed record UpdateMenuPlanItemRequest(
    DateOnly Date,
    string MealType,
    Guid? RecipeId,
    string? RecipeTitle,
    string? Text,
    int Servings,
    string? Comment);
