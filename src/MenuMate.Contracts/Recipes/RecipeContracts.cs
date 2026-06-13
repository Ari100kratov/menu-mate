#pragma warning disable CS1573

namespace MenuMate.Contracts.Recipes;

/// <summary>
/// Запрос на создание рецепта.
/// </summary>
/// <param name="Title">Название блюда.</param>
/// <param name="Description">Краткое описание.</param>
/// <param name="Servings">Количество персон в исходном рецепте.</param>
/// <param name="Category">Основная категория блюда.</param>
/// <param name="TotalTimeMinutes">Общее время приготовления в минутах.</param>
/// <param name="ActiveTimeMinutes">Активное время приготовления в минутах.</param>
/// <param name="SourceUrl">URL источника рецепта.</param>
/// <param name="Ingredients">Ингредиенты рецепта.</param>
/// <param name="Steps">Шаги приготовления.</param>
/// <param name="Tags">Теги рецепта.</param>
public sealed record CreateRecipeRequest(
    string Title,
    string? Description,
    int Servings,
    string Category,
    string Visibility,
    int? TotalTimeMinutes,
    int? ActiveTimeMinutes,
    Uri? SourceUrl,
    IReadOnlyCollection<RecipeIngredientRequest> Ingredients,
    IReadOnlyCollection<PreparationStepRequest> Steps,
    IReadOnlyCollection<string> Tags);

/// <summary>
/// Запрос на обновление рецепта.
/// </summary>
/// <param name="Title">Название блюда.</param>
/// <param name="Description">Краткое описание.</param>
/// <param name="Servings">Количество персон в исходном рецепте.</param>
/// <param name="Category">Основная категория блюда.</param>
/// <param name="TotalTimeMinutes">Общее время приготовления в минутах.</param>
/// <param name="ActiveTimeMinutes">Активное время приготовления в минутах.</param>
/// <param name="SourceUrl">URL источника рецепта.</param>
/// <param name="Ingredients">Ингредиенты рецепта.</param>
/// <param name="Steps">Шаги приготовления.</param>
/// <param name="Tags">Теги рецепта.</param>
public sealed record UpdateRecipeRequest(
    string Title,
    string? Description,
    int Servings,
    string Category,
    string Visibility,
    int? TotalTimeMinutes,
    int? ActiveTimeMinutes,
    Uri? SourceUrl,
    IReadOnlyCollection<RecipeIngredientRequest> Ingredients,
    IReadOnlyCollection<PreparationStepRequest> Steps,
    IReadOnlyCollection<string> Tags);

/// <summary>
/// Ингредиент рецепта во входящем API-запросе.
/// </summary>
/// <param name="IngredientId">Идентификатор продукта общего каталога.</param>
/// <param name="ProductName">Название продукта.</param>
/// <param name="Amount">Количество, если оно числовое.</param>
/// <param name="Unit">Единица измерения.</param>
/// <param name="Category">Категория продукта для списка покупок.</param>
/// <param name="Comment">Комментарий.</param>
/// <param name="IsOptional">Признак необязательного ингредиента.</param>
public sealed record RecipeIngredientRequest(
    Guid? IngredientId,
    string ProductName,
    decimal? Amount,
    string Unit,
    string Category,
    string? Comment,
    bool IsOptional);

/// <summary>
/// Шаг приготовления во входящем API-запросе.
/// </summary>
/// <param name="Text">Текст шага.</param>
public sealed record PreparationStepRequest(string Text);

/// <summary>
/// Краткая карточка рецепта для списков.
/// </summary>
/// <param name="Id">Идентификатор рецепта.</param>
/// <param name="Title">Название блюда.</param>
/// <param name="Description">Краткое описание.</param>
/// <param name="Servings">Количество персон в исходном рецепте.</param>
/// <param name="Category">Основная категория блюда.</param>
/// <param name="TotalTimeMinutes">Общее время приготовления в минутах.</param>
/// <param name="ActiveTimeMinutes">Активное время приготовления в минутах.</param>
/// <param name="IsFavorite">Признак избранного рецепта.</param>
/// <param name="Tags">Теги рецепта.</param>
/// <param name="CoverImage">Активная обложка рецепта.</param>
public sealed record RecipeListItemResponse(
    Guid Id,
    Guid CurrentRevisionId,
    int RevisionNumber,
    bool IsOwnedByCurrentUser,
    bool IsSaved,
    string Title,
    string? Description,
    int Servings,
    string Category,
    string Visibility,
    int? TotalTimeMinutes,
    int? ActiveTimeMinutes,
    bool IsFavorite,
    IReadOnlyCollection<string> Tags,
    RecipeImageResponse? CoverImage);

/// <summary>
/// Рецепт, возвращаемый внешним API.
/// </summary>
/// <param name="Id">Идентификатор рецепта.</param>
/// <param name="Title">Название блюда.</param>
/// <param name="Description">Краткое описание.</param>
/// <param name="Servings">Количество персон в исходном рецепте.</param>
/// <param name="Category">Основная категория блюда.</param>
/// <param name="TotalTimeMinutes">Общее время приготовления в минутах.</param>
/// <param name="ActiveTimeMinutes">Активное время приготовления в минутах.</param>
/// <param name="IsFavorite">Признак избранного рецепта.</param>
/// <param name="SourceUrl">URL источника рецепта.</param>
/// <param name="Tags">Теги рецепта.</param>
/// <param name="Images">Изображения, привязанные к рецепту.</param>
/// <param name="Ingredients">Ингредиенты рецепта.</param>
/// <param name="Steps">Шаги приготовления.</param>
public sealed record RecipeResponse(
    Guid Id,
    Guid CurrentRevisionId,
    int RevisionNumber,
    bool IsOwnedByCurrentUser,
    bool IsSaved,
    Guid? SourceRecipeId,
    Guid? SourceRevisionId,
    string Title,
    string? Description,
    int Servings,
    string Category,
    string Visibility,
    int? TotalTimeMinutes,
    int? ActiveTimeMinutes,
    bool IsFavorite,
    Uri? SourceUrl,
    IReadOnlyCollection<string> Tags,
    IReadOnlyCollection<RecipeImageResponse> Images,
    IReadOnlyCollection<IngredientResponse> Ingredients,
    IReadOnlyCollection<PreparationStepResponse> Steps);

/// <summary>
/// Изображение, привязанное к рецепту или его части.
/// </summary>
/// <param name="Id">Идентификатор изображения.</param>
/// <param name="Scope">Область привязки изображения: Cover или Step.</param>
/// <param name="StepNumber">Номер шага, если изображение привязано к шагу.</param>
/// <param name="BucketName">Имя бакета MinIO.</param>
/// <param name="ObjectKey">Ключ объекта в MinIO.</param>
/// <param name="ContentType">MIME-тип изображения.</param>
/// <param name="SizeBytes">Размер файла в байтах.</param>
/// <param name="AltText">Альтернативное описание изображения.</param>
/// <param name="ReadUrl">Временная или публичная ссылка для прямого чтения из MinIO.</param>
public sealed record RecipeImageResponse(
    Guid Id,
    string Scope,
    int? StepNumber,
    string BucketName,
    string ObjectKey,
    string ContentType,
    long SizeBytes,
    string? AltText,
    Uri? ReadUrl);

/// <summary>
/// Ингредиент рецепта во внешнем контракте.
/// </summary>
/// <param name="IngredientId">Идентификатор продукта общего каталога.</param>
/// <param name="ProductName">Название продукта.</param>
/// <param name="Amount">Количество, если оно числовое.</param>
/// <param name="Unit">Единица измерения.</param>
/// <param name="Comment">Комментарий к ингредиенту.</param>
/// <param name="IsOptional">Признак необязательного ингредиента.</param>
/// <param name="Category">Категория продукта для списка покупок.</param>
public sealed record IngredientResponse(
    Guid? IngredientId,
    string ProductName,
    decimal? Amount,
    string Unit,
    string? Comment,
    bool IsOptional,
    string Category);

/// <summary>
/// Canonical ingredient available for recipe ingredient autocomplete.
/// </summary>
public sealed record CatalogIngredientResponse(
    Guid Id,
    string Name,
    string Category);

/// <summary>
/// Шаг приготовления во внешнем контракте.
/// </summary>
/// <param name="Number">Порядковый номер шага.</param>
/// <param name="Text">Текст шага.</param>
public sealed record PreparationStepResponse(int Number, string Text);
