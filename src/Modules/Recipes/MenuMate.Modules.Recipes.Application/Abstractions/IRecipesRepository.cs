using MenuMate.Modules.Recipes.Domain.Models;

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
}
