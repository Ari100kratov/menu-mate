using MenuMate.Modules.ShoppingLists.Domain.Models;

namespace MenuMate.Modules.ShoppingLists.Application.Abstractions;

internal interface IShoppingListsRepository
{
    Task AddAsync(SavedShoppingList shoppingList, CancellationToken cancellationToken);

    Task<SavedShoppingList?> GetByIdAsync(Guid shoppingListId, CancellationToken cancellationToken);

    Task UpdateAsync(SavedShoppingList shoppingList, CancellationToken cancellationToken);

    Task DeleteAsync(SavedShoppingList shoppingList, CancellationToken cancellationToken);
}
