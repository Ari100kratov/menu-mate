using MenuMate.Common.Application;
using MenuMate.Contracts.ShoppingLists;
using MenuMate.Modules.ShoppingLists.Application.Abstractions;
using MenuMate.Modules.ShoppingLists.Domain.Models;
using MenuMate.SharedKernel;

namespace MenuMate.Modules.ShoppingLists.Application.SetShoppingListItemState;

internal sealed class SetShoppingListItemStateCommandHandler(
    IShoppingListsRepository repository,
    IShoppingListsReadDbContext readDbContext,
    IShoppingListsUnitOfWork unitOfWork,
    IUserContext userContext,
    TimeProvider timeProvider)
    : ICommandHandler<SetShoppingListItemStateCommand, ShoppingListResponse>
{
    public async Task<Result<ShoppingListResponse>> Handle(
        SetShoppingListItemStateCommand command,
        CancellationToken cancellationToken)
    {
        SavedShoppingList? shoppingList = await repository.GetByOwnerAsync(userContext.UserId, cancellationToken);
        if (shoppingList is null)
        {
            return Result.Failure<ShoppingListResponse>(ShoppingListApplicationErrors.EmptyList);
        }

        if (!shoppingList.SetItemState(
                command.ItemId,
                command.Request.IsPurchased,
                timeProvider.GetUtcNow()))
        {
            return Result.Failure<ShoppingListResponse>(ShoppingListApplicationErrors.ItemNotFound(command.ItemId));
        }

        await repository.UpdateAsync(shoppingList, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        ShoppingListResponse? response = await readDbContext.GetShoppingListAsync(
            shoppingList.Id,
            userContext.UserId,
            cancellationToken);

        return response is null
            ? Result.Failure<ShoppingListResponse>(ShoppingListApplicationErrors.NotFound(shoppingList.Id))
            : response;
    }
}
