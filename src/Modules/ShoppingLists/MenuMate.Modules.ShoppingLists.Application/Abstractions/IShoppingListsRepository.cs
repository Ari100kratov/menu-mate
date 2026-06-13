using MenuMate.Modules.ShoppingLists.Domain.Models;
using MenuMate.SharedKernel.Identifiers;

namespace MenuMate.Modules.ShoppingLists.Application.Abstractions;

internal interface IShoppingListsRepository
{
    Task AddAsync(SavedShoppingList shoppingList, CancellationToken cancellationToken);

    Task<SavedShoppingList?> GetByOwnerAsync(UserId ownerUserId, CancellationToken cancellationToken);

    Task UpdateAsync(SavedShoppingList shoppingList, CancellationToken cancellationToken);

}
