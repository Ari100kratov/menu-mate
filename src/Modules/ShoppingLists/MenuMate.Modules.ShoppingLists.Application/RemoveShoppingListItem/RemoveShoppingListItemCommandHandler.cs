using MenuMate.Common.Application;
using MenuMate.Modules.ShoppingLists.Application.Abstractions;
using MenuMate.Modules.ShoppingLists.Domain.Models;
using MenuMate.SharedKernel;

namespace MenuMate.Modules.ShoppingLists.Application.RemoveShoppingListItem;

internal sealed class RemoveShoppingListItemCommandHandler(
    IShoppingListsRepository repository,
    IShoppingListsUnitOfWork unitOfWork,
    IUserContext userContext,
    TimeProvider timeProvider)
    : ICommandHandler<RemoveShoppingListItemCommand>
{
    public async Task<Result> Handle(RemoveShoppingListItemCommand command, CancellationToken cancellationToken)
    {
        SavedShoppingList? shoppingList = await repository.GetByOwnerAsync(userContext.UserId, cancellationToken);
        if (shoppingList is null)
        {
            return Result.Failure(ShoppingListApplicationErrors.EmptyList);
        }

        if (!shoppingList.RemoveItem(command.ItemId, timeProvider.GetUtcNow()))
        {
            return Result.Failure(ShoppingListApplicationErrors.ItemNotFound(command.ItemId));
        }

        await repository.UpdateAsync(shoppingList, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
