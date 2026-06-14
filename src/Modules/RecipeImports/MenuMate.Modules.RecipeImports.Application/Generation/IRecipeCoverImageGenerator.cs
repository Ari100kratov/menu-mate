using MenuMate.Contracts.Recipes;

namespace MenuMate.Modules.RecipeImports.Application.Generation;

/// <summary>Генерирует обложку блюда по данным рецепта.</summary>
public interface IRecipeCoverImageGenerator
{
    /// <summary>Создаёт изображение блюда.</summary>
    Task<GeneratedRecipeCoverImage> GenerateAsync(
        CreateRecipeRequest recipe,
        CancellationToken cancellationToken);
}

/// <summary>Сгенерированная обложка рецепта.</summary>
public sealed record GeneratedRecipeCoverImage(
    ReadOnlyMemory<byte> Content,
    string ContentType,
    string FileName);
