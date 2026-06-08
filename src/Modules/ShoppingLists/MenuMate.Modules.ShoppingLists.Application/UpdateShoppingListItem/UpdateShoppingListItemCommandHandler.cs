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
        SavedShoppingList? shoppingList = await repository.GetByIdAsync(command.ShoppingListId, cancellationToken);
        if (shoppingList is null)
        {
            return Result.Failure<ShoppingListResponse>(ShoppingListApplicationErrors.NotFound(command.ShoppingListId));
        }

        if (shoppingList.OwnerUserId != userContext.UserId)
        {
            return Result.Failure<ShoppingListResponse>(ShoppingListApplicationErrors.AccessDenied);
        }

        Result<SavedShoppingListItem> item = await productResolver.ResolveAsync(
            command.ItemId,
            command.Request,
            cancellationToken);
        if (item.IsFailure)
        {
            return Result.Failure<ShoppingListResponse>(item.Error);
        }

        if (!shoppingList.UpdateItem(command.ItemId, item.Value, timeProvider.GetUtcNow()))
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
