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
    TimeProvider timeProvider,
    ShoppingProductResolver productResolver)
    : ICommandHandler<AddShoppingListItemCommand, ShoppingListResponse>
{
    public async Task<Result<ShoppingListResponse>> Handle(
        AddShoppingListItemCommand command,
        CancellationToken cancellationToken)
    {
        SavedShoppingList? shoppingList = await repository.GetByOwnerAsync(userContext.UserId, cancellationToken);
        bool isNew = shoppingList is null;
        if (shoppingList is null)
        {
            DateTimeOffset now = timeProvider.GetUtcNow();
            var today = DateOnly.FromDateTime(now.UtcDateTime);
            Result<SavedShoppingList> created = SavedShoppingList.Create(
                Guid.CreateVersion7(),
                userContext.UserId,
                today,
                today,
                [],
                now);
            if (created.IsFailure)
            {
                return Result.Failure<ShoppingListResponse>(created.Error);
            }

            shoppingList = created.Value;
        }

        Result<SavedShoppingListItem> item = await productResolver.ResolveAsync(
            Guid.CreateVersion7(),
            command.Request,
            cancellationToken);

        if (item.IsFailure)
        {
            return Result.Failure<ShoppingListResponse>(item.Error);
        }

        shoppingList.AddItem(item.Value, timeProvider.GetUtcNow());
        if (isNew)
        {
            await repository.AddAsync(shoppingList, cancellationToken);
        }
        else
        {
            await repository.UpdateAsync(shoppingList, cancellationToken);
        }
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
