using MenuMate.Contracts.ShoppingLists;
using MenuMate.SharedKernel.Identifiers;

namespace MenuMate.Modules.ShoppingLists.Application.Abstractions;

/// <summary>
/// Контракт чтения списков покупок через проекции без гидрации агрегатов.
/// </summary>
internal interface IShoppingListsReadDbContext
{
    /// <summary>
    /// Возвращает список покупок владельца.
    /// </summary>
    Task<ShoppingListResponse?> GetShoppingListAsync(
        Guid shoppingListId,
        UserId ownerUserId,
        CancellationToken cancellationToken);

    Task<ShoppingListResponse?> GetCurrentShoppingListAsync(
        UserId ownerUserId,
        CancellationToken cancellationToken);
}
