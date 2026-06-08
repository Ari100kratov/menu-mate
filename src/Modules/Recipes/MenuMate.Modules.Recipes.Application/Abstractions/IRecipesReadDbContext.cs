using MenuMate.Contracts.Recipes;
using MenuMate.SharedKernel.Identifiers;

namespace MenuMate.Modules.Recipes.Application.Abstractions;

/// <summary>
/// Контракт чтения рецептов через EF-проекции без гидрации доменных агрегатов.
/// </summary>
internal interface IRecipesReadDbContext
{
    /// <summary>
    /// Возвращает детальную карточку рецепта владельца.
    /// </summary>
    Task<RecipeResponse?> GetRecipeAsync(
        Guid recipeId,
        UserId currentUserId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Возвращает список рецептов владельца с базовой фильтрацией.
    /// </summary>
    Task<IReadOnlyCollection<RecipeListItemResponse>> GetRecipesAsync(
        UserId currentUserId,
        bool catalog,
        string? search,
        string? normalizedTag,
        bool favoritesOnly,
        CancellationToken cancellationToken);
}
