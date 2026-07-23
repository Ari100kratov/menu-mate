using MenuMate.Common.Application;
using MenuMate.Contracts.ShoppingLists;
using MenuMate.Modules.ShoppingLists.Application.Abstractions;
using MenuMate.SharedKernel;

namespace MenuMate.Modules.ShoppingLists.Application.GetCurrentShoppingList;

internal sealed class GetCurrentShoppingListQueryHandler(
    IShoppingListsReadDbContext dbContext,
    IUserContext userContext,
    TimeProvider timeProvider)
    : IQueryHandler<GetCurrentShoppingListQuery, ShoppingListResponse>
{
    public async Task<Result<ShoppingListResponse>> Handle(
        GetCurrentShoppingListQuery query,
        CancellationToken cancellationToken)
    {
        ShoppingListResponse? response = await dbContext.GetCurrentShoppingListAsync(
            userContext.UserId,
            cancellationToken);
        if (response is not null)
        {
            return response;
        }

        DateTimeOffset now = timeProvider.GetUtcNow();
        return new ShoppingListResponse(Guid.Empty, null, null, now, now, [], string.Empty);
    }
}
