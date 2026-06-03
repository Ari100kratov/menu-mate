using MenuMate.Common.Application;
using MenuMate.Contracts.ShoppingLists;
using MenuMate.Modules.ShoppingLists.Application.Abstractions;
using MenuMate.SharedKernel;

namespace MenuMate.Modules.ShoppingLists.Application.GetShoppingListById;

internal sealed class GetShoppingListByIdQueryHandler(
    IShoppingListsReadDbContext dbContext,
    IUserContext userContext)
    : IQueryHandler<GetShoppingListByIdQuery, ShoppingListResponse>
{
    public async Task<Result<ShoppingListResponse>> Handle(
        GetShoppingListByIdQuery query,
        CancellationToken cancellationToken)
    {
        ShoppingListResponse? shoppingList = await dbContext.GetShoppingListAsync(
            query.ShoppingListId,
            userContext.UserId,
            cancellationToken);

        return shoppingList is null
            ? Result.Failure<ShoppingListResponse>(ShoppingListApplicationErrors.NotFound(query.ShoppingListId))
            : shoppingList;
    }
}
