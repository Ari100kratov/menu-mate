using MenuMate.Modules.ShoppingLists.Domain.Models;
using MenuMate.SharedKernel.Identifiers;

namespace MenuMate.Modules.ShoppingLists.Application.Abstractions;

/// <summary>
/// Контракт чтения данных меню и рецептов для генерации списка покупок.
/// </summary>
internal interface IShoppingListSourceReader
{
    /// <summary>
    /// Возвращает рецепты из плана меню, доступного текущему владельцу.
    /// </summary>
    Task<IReadOnlyCollection<ShoppingRecipe>?> GetMenuPlanRecipesAsync(
        MenuPlanId menuPlanId,
        UserId ownerUserId,
        CancellationToken cancellationToken);
}
