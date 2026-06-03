namespace MenuMate.Modules.ShoppingLists.Application.Abstractions;

internal interface IShoppingListsUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
