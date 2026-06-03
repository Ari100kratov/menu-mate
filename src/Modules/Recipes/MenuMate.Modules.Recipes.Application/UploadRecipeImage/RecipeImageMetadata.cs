using MenuMate.Contracts.Recipes;
using MenuMate.Modules.Recipes.Domain.Enums;
using MenuMate.SharedKernel.Identifiers;

namespace MenuMate.Modules.Recipes.Application.UploadRecipeImage;

/// <summary>
/// Метаданные изображения рецепта, сохраняемые в базе модуля Recipes.
/// </summary>
internal sealed record RecipeImageMetadata(
    Guid Id,
    UserId OwnerUserId,
    Guid RecipeId,
    RecipeImageScope Scope,
    int? StepNumber,
    string BucketName,
    string ObjectKey,
    string ContentType,
    long SizeBytes,
    string? OriginalFileName,
    string? AltText,
    DateTimeOffset CreatedAt)
{
    /// <summary>
    /// Возвращает внешний API-контракт изображения.
    /// </summary>
    public RecipeImageResponse ToResponse(Uri? readUrl) =>
        new(
            Id,
            Scope.ToString(),
            StepNumber,
            BucketName,
            ObjectKey,
            ContentType,
            SizeBytes,
            AltText,
            readUrl);
}
