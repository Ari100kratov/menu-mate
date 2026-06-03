using MenuMate.Common.Application;
using MenuMate.Contracts.ShoppingLists;
using MenuMate.Modules.ShoppingLists.Application.Abstractions;
using MenuMate.SharedKernel;

namespace MenuMate.Modules.ShoppingLists.Application.GetShoppingLists;

internal sealed class GetShoppingListsQueryHandler(
    IShoppingListsReadDbContext dbContext,
    IUserContext userContext)
    : IQueryHandler<GetShoppingListsQuery, IReadOnlyCollection<ShoppingListSummaryResponse>>
{
    public async Task<Result<IReadOnlyCollection<ShoppingListSummaryResponse>>> Handle(
        GetShoppingListsQuery query,
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<ShoppingListSummaryResponse> shoppingLists = await dbContext.GetShoppingListsAsync(
            userContext.UserId,
            cancellationToken);

        return shoppingLists.ToArray();
    }
}
