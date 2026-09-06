using MenuMate.Common.Application;
using MenuMate.SharedKernel.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace MenuMate.Modules.ShoppingLists.Infrastructure.Database;

internal sealed class ShoppingListsUserDataEraser(ShoppingListsDbContext dbContext) : IUserDataEraser
{
    public int Order => 300;

    public async Task EraseAsync(UserId userId, CancellationToken cancellationToken)
    {
        await dbContext.ShoppingLists
            .Where(shoppingList => shoppingList.OwnerUserId == userId)
            .ExecuteDeleteAsync(cancellationToken);
    }
}
