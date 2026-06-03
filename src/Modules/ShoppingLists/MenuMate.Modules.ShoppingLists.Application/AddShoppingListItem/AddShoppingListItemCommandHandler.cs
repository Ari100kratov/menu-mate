using MenuMate.Common.Application;
using MenuMate.Contracts.ShoppingLists;
using MenuMate.Modules.ShoppingLists.Application.Abstractions;
using MenuMate.Modules.ShoppingLists.Domain.Models;
using MenuMate.SharedKernel;

namespace MenuMate.Modules.ShoppingLists.Application.AddShoppingListItem;

internal sealed class AddShoppingListItemCommandHandler(
    IShoppingListsRepository repository,
    IShoppingListsReadDbContext readDbContext,
    IShoppingListsUnitOfWork unitOfWork,
    IUserContext userContext,
    TimeProvider timeProvider)
    : ICommandHandler<AddShoppingListItemCommand, ShoppingListResponse>
{
    public async Task<Result<ShoppingListResponse>> Handle(
        AddShoppingListItemCommand command,
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

        Result<SavedShoppingListItem> item = ShoppingListItemRequestMapper.Map(
            Guid.CreateVersion7(),
            command.Request);

        if (item.IsFailure)
        {
            return Result.Failure<ShoppingListResponse>(item.Error);
        }

        shoppingList.AddItem(item.Value, timeProvider.GetUtcNow());
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
