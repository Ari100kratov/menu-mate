using MenuMate.Contracts.Recipes;
using MenuMate.Modules.Recipes.Domain.Enums;
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
    Task<RecipeReadModel?> GetRecipeAsync(
        Guid recipeId,
        UserId currentUserId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Возвращает список рецептов владельца с базовой фильтрацией.
    /// </summary>
    Task<IReadOnlyCollection<RecipeListItemReadModel>> GetRecipesAsync(
        UserId currentUserId,
        bool catalog,
        string? search,
        IReadOnlyCollection<Guid> tagIds,
        RecipeCategory? category,
        bool favoritesOnly,
        int skip,
        int take,
        CancellationToken cancellationToken);

}
