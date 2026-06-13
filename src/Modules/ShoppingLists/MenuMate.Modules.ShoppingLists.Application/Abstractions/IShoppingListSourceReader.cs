using MenuMate.Modules.ShoppingLists.Domain.Models;
using MenuMate.SharedKernel.Identifiers;

namespace MenuMate.Modules.ShoppingLists.Application.Abstractions;

/// <summary>
/// Контракт чтения данных меню и рецептов для генерации списка покупок.
/// </summary>
internal interface IShoppingListSourceReader
{
    /// <summary>
    /// Возвращает рецепты из диапазона календаря меню текущего владельца.
    /// </summary>
    Task<IReadOnlyCollection<ShoppingRecipe>> GetMenuCalendarRecipesAsync(
        UserId ownerUserId,
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken);
}
