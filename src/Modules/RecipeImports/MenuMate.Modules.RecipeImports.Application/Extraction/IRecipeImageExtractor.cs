using MenuMate.Contracts.Recipes;

namespace MenuMate.Modules.RecipeImports.Application.Extraction;

/// <summary>
/// Извлекает предлагаемые поля рецепта из изображения.
/// </summary>
public interface IRecipeImageExtractor
{
    /// <summary>Распознает изображение и возвращает структурированный черновик.</summary>
    Task<RecipeImageExtractionResult> ExtractAsync(
        IReadOnlyCollection<RecipeImageInput> images,
        CancellationToken cancellationToken);
}

/// <summary>Изображение для распознавания рецепта.</summary>
public sealed record RecipeImageInput(ReadOnlyMemory<byte> Content, string ContentType);

/// <summary>
/// Результат распознавания изображения рецепта.
/// </summary>
public sealed record RecipeImageExtractionResult(
    CreateRecipeRequest Recipe,
    string ExtractedText,
    IReadOnlyCollection<string> Warnings,
    string Provider,
    string Model,
    string? ProviderResponseId);
