namespace MenuMate.Modules.RecipeImports.Domain.Models;

/// <summary>
/// Исходное изображение, использованное для распознавания рецепта.
/// </summary>
public sealed record RecipeImportSourceImage(
    string BucketName,
    string ObjectKey,
    string ContentType,
    long SizeBytes,
    string FileName);
