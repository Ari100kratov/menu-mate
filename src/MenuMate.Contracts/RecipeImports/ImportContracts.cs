using MenuMate.Contracts.Recipes;

namespace MenuMate.Contracts.RecipeImports;

/// <summary>
/// Краткая карточка черновика импорта рецепта.
/// </summary>
public sealed record RecipeImportDraftListItemResponse(
    Guid Id,
    string Status,
    string Title,
    Guid? CreatedRecipeId,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

/// <summary>
/// Доказательства и служебные сведения распознавания рецепта.
/// </summary>
public sealed record RecipeImportEvidenceResponse(
    string ExtractedText,
    IReadOnlyCollection<string> Warnings,
    string Provider,
    string Model,
    string? ProviderResponseId,
    RecipeImportSuggestedCoverResponse? SuggestedCover = null);

/// <summary>
/// Предложенная область исходного изображения с фотографией готового блюда.
/// Координаты нормализованы относительно сторон изображения в диапазоне от 0 до 1000.
/// </summary>
public sealed record RecipeImportSuggestedCoverResponse(
    int SourceImageIndex,
    int X,
    int Y,
    int Width,
    int Height,
    int Confidence);

/// <summary>
/// Исходное изображение черновика импорта.
/// </summary>
public sealed record RecipeImportSourceImageResponse(
    Uri? ReadUrl,
    string ContentType,
    long SizeBytes,
    string FileName);

/// <summary>
/// Полный черновик импорта рецепта.
/// </summary>
public sealed record RecipeImportDraftResponse(
    Guid Id,
    string Status,
    CreateRecipeRequest Recipe,
    RecipeImportEvidenceResponse Evidence,
    IReadOnlyCollection<RecipeImportSourceImageResponse> SourceImages,
    Guid? CreatedRecipeId,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

/// <summary>
/// Запрос на замену редактируемого содержимого черновика.
/// </summary>
public sealed record UpdateRecipeImportDraftRequest(CreateRecipeRequest Recipe);
