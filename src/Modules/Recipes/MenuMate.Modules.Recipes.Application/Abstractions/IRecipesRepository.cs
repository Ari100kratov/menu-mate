using MenuMate.Modules.Recipes.Domain.Models;
using MenuMate.SharedKernel.Identifiers;

namespace MenuMate.Modules.Recipes.Application.Abstractions;

/// <summary>
/// Хранилище рецептов, принадлежащее модулю Recipes.
/// </summary>
public interface IRecipesRepository
{
    /// <summary>
    /// Добавляет рецепт в хранилище.
    /// </summary>
    Task AddAsync(Recipe recipe, CancellationToken cancellationToken);

    /// <summary>
    /// Возвращает рецепт по идентификатору.
    /// </summary>
    Task<Recipe?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Сохраняет новый снимок рецепта как измененный.
    /// </summary>
    Task UpdateAsync(Recipe recipe, CancellationToken cancellationToken);

    /// <summary>Saves a recipe to a user's library and sets its favorite state.</summary>
    Task SaveToLibraryAsync(
        Guid recipeId,
        RecipeRevisionId recipeRevisionId,
        UserId userId,
        DateTimeOffset savedAt,
        CancellationToken cancellationToken);

    /// <summary>Removes a recipe from a user's library.</summary>
    Task RemoveFromLibraryAsync(Guid recipeId, UserId userId, CancellationToken cancellationToken);

}
