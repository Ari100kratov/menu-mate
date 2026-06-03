namespace MenuMate.Contracts.ShoppingLists;

/// <summary>
/// Краткая карточка сохраненного списка покупок.
/// </summary>
/// <param name="Id">Идентификатор списка покупок.</param>
/// <param name="SourceMenuPlanId">Идентификатор плана меню, по которому создан список.</param>
/// <param name="CreatedAt">Момент создания списка.</param>
/// <param name="UpdatedAt">Момент последнего изменения списка.</param>
/// <param name="ItemsCount">Количество позиций.</param>
/// <param name="PurchasedItemsCount">Количество купленных позиций.</param>
public sealed record ShoppingListSummaryResponse(
    Guid Id,
    Guid SourceMenuPlanId,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    int ItemsCount,
    int PurchasedItemsCount);

/// <summary>
/// Список покупок во внешнем контракте.
/// </summary>
/// <param name="Id">Идентификатор списка покупок.</param>
/// <param name="SourceMenuPlanId">Идентификатор плана меню, по которому создан список.</param>
/// <param name="CreatedAt">Момент создания списка.</param>
/// <param name="UpdatedAt">Момент последнего изменения списка.</param>
/// <param name="Categories">Категории списка покупок.</param>
/// <param name="Text">Текстовая версия для копирования.</param>
public sealed record ShoppingListResponse(
    Guid Id,
    Guid SourceMenuPlanId,
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
/// <param name="Name">Название продукта.</param>
/// <param name="Amount">Числовое количество.</param>
/// <param name="Unit">Единица измерения.</param>
/// <param name="QuantityKind">Тип количества.</param>
/// <param name="Category">Категория продукта.</param>
/// <param name="AmountText">Количество в человекочитаемом виде.</param>
/// <param name="Comment">Комментарий.</param>
/// <param name="IsPurchased">Признак купленного продукта.</param>
/// <param name="IsInStock">Признак продукта, который уже есть дома.</param>
public sealed record ShoppingListItemResponse(
    Guid Id,
    string Name,
    decimal? Amount,
    string Unit,
    string QuantityKind,
    string Category,
    string AmountText,
    string? Comment,
    bool IsPurchased,
    bool IsInStock);

/// <summary>
/// Запрос на генерацию списка покупок из плана меню.
/// </summary>
/// <param name="MenuPlanId">Идентификатор плана меню.</param>
/// <param name="ManualItems">Дополнительные ручные позиции.</param>
public sealed record GenerateShoppingListRequest(
    Guid MenuPlanId,
    IReadOnlyCollection<ShoppingListItemRequest> ManualItems);

/// <summary>
/// Запрос на создание или обновление позиции списка покупок.
/// </summary>
/// <param name="Name">Название продукта.</param>
/// <param name="Amount">Числовое количество.</param>
/// <param name="Unit">Единица измерения.</param>
/// <param name="QuantityKind">Тип количества.</param>
/// <param name="Category">Категория продукта.</param>
/// <param name="Comment">Комментарий.</param>
public sealed record ShoppingListItemRequest(
    string Name,
    decimal? Amount,
    string Unit,
    string QuantityKind,
    string Category,
    string? Comment);

/// <summary>
/// Запрос на обновление чекбоксов позиции.
/// </summary>
/// <param name="IsPurchased">Признак купленного продукта.</param>
/// <param name="IsInStock">Признак продукта, который уже есть дома.</param>
public sealed record ShoppingListItemStateRequest(bool IsPurchased, bool IsInStock);
