using MenuMate.Modules.ShoppingLists.Application.Abstractions;
using MenuMate.Modules.ShoppingLists.Domain.Models;
using MenuMate.Modules.ShoppingLists.Infrastructure.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace MenuMate.Modules.ShoppingLists.Infrastructure.Database;

internal sealed class EfShoppingListsRepository(ShoppingListsDbContext dbContext) : IShoppingListsRepository
{
    public async Task AddAsync(SavedShoppingList shoppingList, CancellationToken cancellationToken)
    {
        await dbContext.ShoppingLists.AddAsync(ShoppingListRecord.FromDomain(shoppingList), cancellationToken);
    }

    public async Task<SavedShoppingList?> GetByIdAsync(Guid shoppingListId, CancellationToken cancellationToken)
    {
        ShoppingListRecord? record = await Query()
            .AsNoTracking()
            .SingleOrDefaultAsync(shoppingList => shoppingList.Id == shoppingListId, cancellationToken);

        return record?.ToDomain();
    }

    public async Task UpdateAsync(SavedShoppingList shoppingList, CancellationToken cancellationToken)
    {
        ShoppingListRecord? record = await Query()
            .SingleOrDefaultAsync(existing => existing.Id == shoppingList.Id, cancellationToken);

        if (record is null)
        {
            await AddAsync(shoppingList, cancellationToken);
            return;
        }

        record.Apply(shoppingList);
    }

    public async Task DeleteAsync(SavedShoppingList shoppingList, CancellationToken cancellationToken)
    {
        ShoppingListRecord? record = await Query()
            .SingleOrDefaultAsync(existing => existing.Id == shoppingList.Id, cancellationToken);

        if (record is not null)
        {
            dbContext.ShoppingLists.Remove(record);
        }
    }

    private IQueryable<ShoppingListRecord> Query() =>
        dbContext.ShoppingLists
            .Include(shoppingList => shoppingList.Items);
}
