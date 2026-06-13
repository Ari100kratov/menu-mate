using MenuMate.Common.Application;
using MenuMate.Contracts.ShoppingLists;
using MenuMate.Modules.ShoppingLists.Application.Abstractions;
using MenuMate.Modules.ShoppingLists.Domain.Models;
using MenuMate.SharedKernel;

namespace MenuMate.Modules.ShoppingLists.Application.UpdateShoppingListItem;

internal sealed class UpdateShoppingListItemCommandHandler(
    IShoppingListsRepository repository,
    IShoppingListsReadDbContext readDbContext,
    IShoppingListsUnitOfWork unitOfWork,
    IUserContext userContext,
    TimeProvider timeProvider,
    ShoppingProductResolver productResolver)
    : ICommandHandler<UpdateShoppingListItemCommand, ShoppingListResponse>
{
    public async Task<Result<ShoppingListResponse>> Handle(
        UpdateShoppingListItemCommand command,
        CancellationToken cancellationToken)
    {
        SavedShoppingList? shoppingList = await repository.GetByOwnerAsync(userContext.UserId, cancellationToken);
        if (shoppingList is null)
        {
            return Result.Failure<ShoppingListResponse>(ShoppingListApplicationErrors.EmptyList);
        }

        SavedShoppingListItem? existingItem = shoppingList.Items.SingleOrDefault(item => item.Id == command.ItemId);
        if (existingItem is null)
        {
            return Result.Failure<ShoppingListResponse>(ShoppingListApplicationErrors.ItemNotFound(command.ItemId));
        }

        Result<SavedShoppingListItem> item = await productResolver.ResolveAsync(
            command.ItemId,
            command.Request,
            cancellationToken);
        if (item.IsFailure)
        {
            return Result.Failure<ShoppingListResponse>(item.Error);
        }

        SavedShoppingListItem updatedItem = item.Value.WithState(existingItem.IsPurchased);
        if (!shoppingList.UpdateItem(command.ItemId, updatedItem, timeProvider.GetUtcNow()))
        {
            return Result.Failure<ShoppingListResponse>(ShoppingListApplicationErrors.ItemNotFound(command.ItemId));
        }

        await repository.UpdateAsync(shoppingList, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return await ReloadAsync(shoppingList.Id, cancellationToken);
    }

    private async Task<Result<ShoppingListResponse>> ReloadAsync(
        Guid shoppingListId,
        CancellationToken cancellationToken)
    {
        ShoppingListResponse? response = await readDbContext.GetShoppingListAsync(
            shoppingListId,
            userContext.UserId,
            cancellationToken);

        return response is null
            ? Result.Failure<ShoppingListResponse>(ShoppingListApplicationErrors.NotFound(shoppingListId))
            : response;
    }
}
