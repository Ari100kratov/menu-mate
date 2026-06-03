namespace MenuMate.Modules.Recipes.Application.Abstractions;

/// <summary>
/// Единица работы модуля Recipes.
/// </summary>
public interface IRecipesUnitOfWork
{
    /// <summary>
    /// Сохраняет изменения модуля Recipes.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}

