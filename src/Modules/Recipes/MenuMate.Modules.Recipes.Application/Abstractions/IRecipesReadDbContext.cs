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
        Guid? revisionId,
        UserId currentUserId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Возвращает доступ к точной ревизии для операций, не требующих полной проекции.
    /// </summary>
    Task<RecipeRevisionAccessReadModel?> GetRevisionAccessAsync(
        Guid recipeId,
        Guid revisionId,
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
        bool availableOnly,
        int skip,
        int take,
        CancellationToken cancellationToken);

}
