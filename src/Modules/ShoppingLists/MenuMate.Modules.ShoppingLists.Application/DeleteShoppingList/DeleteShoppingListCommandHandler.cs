using MenuMate.Common.Application;
using MenuMate.Modules.ShoppingLists.Application.Abstractions;
using MenuMate.Modules.ShoppingLists.Domain.Models;
using MenuMate.SharedKernel;

namespace MenuMate.Modules.ShoppingLists.Application.DeleteShoppingList;

internal sealed class DeleteShoppingListCommandHandler(
    IShoppingListsRepository repository,
    IShoppingListsUnitOfWork unitOfWork,
    IUserContext userContext)
    : ICommandHandler<DeleteShoppingListCommand>
{
    public async Task<Result> Handle(DeleteShoppingListCommand command, CancellationToken cancellationToken)
    {
        SavedShoppingList? shoppingList = await repository.GetByIdAsync(command.ShoppingListId, cancellationToken);
        if (shoppingList is null)
        {
            return Result.Failure(ShoppingListApplicationErrors.NotFound(command.ShoppingListId));
        }

        if (shoppingList.OwnerUserId != userContext.UserId)
        {
            return Result.Failure(ShoppingListApplicationErrors.AccessDenied);
        }

        await repository.DeleteAsync(shoppingList, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
