using MenuMate.Modules.Recipes.Domain.Enums;
using MenuMate.SharedKernel.Identifiers;

namespace MenuMate.Modules.Recipes.Application.RecipeImages;

/// <summary>
/// Ссылка на объект изображения в MinIO, привязанная к metadata-записи рецепта.
/// </summary>
internal sealed record RecipeImageObjectReference(
    Guid Id,
    UserId OwnerUserId,
    Guid RecipeId,
    RecipeImageScope Scope,
    string BucketName,
    string ObjectKey);
