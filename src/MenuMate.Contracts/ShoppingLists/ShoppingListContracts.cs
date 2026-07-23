namespace MenuMate.Contracts.ShoppingLists;

/// <summary>
/// Список покупок во внешнем контракте.
/// </summary>
/// <param name="Id">Идентификатор списка покупок.</param>
/// <param name="SourceStartDate">Дата начала диапазона меню, по которому создан список.</param>
/// <param name="SourceEndDate">Дата окончания диапазона меню, по которому создан список.</param>
/// <param name="CreatedAt">Момент создания списка.</param>
/// <param name="UpdatedAt">Момент последнего изменения списка.</param>
/// <param name="Categories">Категории списка покупок.</param>
/// <param name="Text">Текстовая версия для копирования.</param>
public sealed record ShoppingListResponse(
    Guid Id,
    DateOnly? SourceStartDate,
    DateOnly? SourceEndDate,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    IReadOnlyCollection<ShoppingListCategoryResponse> Categories,
    string Text);

/// <summary>
/// Категория списка покупок.
/// </summary>
/// <param name="Name">Название категории.</param>
/// <param name="Items">Позиции категории.</param>
public sealed record ShoppingListCategoryResponse(string Name, IReadOnlyCollection<ShoppingListItemResponse> Items);

/// <summary>
/// Позиция списка покупок.
/// </summary>
/// <param name="Id">Идентификатор позиции.</param>
/// <param name="ProductId">Идентификатор продукта общего каталога.</param>
/// <param name="Name">Название продукта.</param>
/// <param name="Amount">Числовое количество.</param>
/// <param name="Unit">Единица измерения.</param>
/// <param name="Category">Категория продукта.</param>
/// <param name="AmountText">Количество в человекочитаемом виде.</param>
/// <param name="Comment">Комментарий.</param>
/// <param name="IsPurchased">Признак купленного продукта.</param>
public sealed record ShoppingListItemResponse(
    Guid Id,
    Guid ProductId,
    string Name,
    decimal? Amount,
    string Unit,
    string Category,
    string AmountText,
    string? Comment,
    bool IsPurchased);

/// <summary>
/// Запрос на генерацию списка покупок из диапазона меню.
/// </summary>
/// <param name="StartDate">Дата начала диапазона меню.</param>
/// <param name="EndDate">Дата окончания диапазона меню.</param>
/// <param name="Recipes">Выбранные блюда и ингредиенты из меню.</param>
public sealed record GenerateShoppingListRequest(
    DateOnly StartDate,
    DateOnly EndDate,
    IReadOnlyCollection<MenuShoppingSelectionRequest> Recipes);

/// <summary>
/// Выбор ингредиентов одного блюда меню для нового списка покупок.
/// </summary>
public sealed record MenuShoppingSelectionRequest(
    Guid MenuItemId,
    int Servings,
    IReadOnlyCollection<Guid> IngredientIds);

/// <summary>
/// Предпросмотр списка покупок по выбранному диапазону меню.
/// </summary>
public sealed record MenuShoppingPreviewResponse(
    DateOnly StartDate,
    DateOnly EndDate,
    IReadOnlyCollection<MenuShoppingPreviewRecipeResponse> Recipes);

/// <summary>
/// Блюдо меню в предпросмотре списка покупок.
/// </summary>
public sealed record MenuShoppingPreviewRecipeResponse(
    Guid MenuItemId,
    string Title,
    int BaseServings,
    int Servings,
    IReadOnlyCollection<MenuShoppingPreviewIngredientResponse> Ingredients);

/// <summary>
/// Ингредиент блюда в предпросмотре списка покупок.
/// </summary>
public sealed record MenuShoppingPreviewIngredientResponse(
    Guid IngredientId,
    Guid ProductId,
    string Name,
    decimal? Amount,
    string Unit,
    string Category,
    string AmountText,
    string? Comment,
    bool IsOptional);

/// <summary>
/// Запрос на создание или обновление позиции списка покупок.
/// </summary>
/// <param name="ProductId">Идентификатор продукта общего каталога.</param>
/// <param name="Name">Название продукта.</param>
/// <param name="Amount">Числовое количество.</param>
/// <param name="Unit">Единица измерения.</param>
/// <param name="Category">Категория продукта.</param>
/// <param name="Comment">Комментарий.</param>
public sealed record ShoppingListItemRequest(
    Guid? ProductId,
    string Name,
    decimal? Amount,
    string Unit,
    string Category,
    string? Comment);

/// <summary>
/// Запрос на обновление чекбоксов позиции.
/// </summary>
/// <param name="IsPurchased">Признак купленного продукта.</param>
public sealed record ShoppingListItemStateRequest(bool IsPurchased);
