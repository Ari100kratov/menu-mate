using MenuMate.Modules.Recipes.Application.UploadRecipeImage;
using MenuMate.Modules.Recipes.Application.RecipeImages;
using MenuMate.Modules.Recipes.Domain.Enums;
using MenuMate.SharedKernel.Identifiers;

namespace MenuMate.Modules.Recipes.Application.Abstractions;

/// <summary>
/// Хранилище метаданных изображений рецептов, принадлежащее модулю Recipes.
/// </summary>
internal interface IRecipeImagesRepository
{
    /// <summary>
    /// Добавляет метаданные изображения рецепта.
    /// </summary>
    Task AddAsync(RecipeImageMetadata image, CancellationToken cancellationToken);

    /// <summary>
    /// Помечает активные изображения указанной области удаленными и возвращает ссылки на их объекты.
    /// </summary>
    Task<IReadOnlyCollection<RecipeImageObjectReference>> MarkActiveImagesDeletedAsync(
        Guid recipeId,
        UserId ownerUserId,
        RecipeImageScope scope,
        int? stepNumber,
        CancellationToken cancellationToken);

    /// <summary>
    /// Помечает активное изображение удаленным и возвращает ссылку на его объект.
    /// </summary>
    Task<RecipeImageObjectReference?> MarkActiveImageDeletedAsync(
        Guid recipeId,
        Guid imageId,
        UserId ownerUserId,
        CancellationToken cancellationToken);
}
