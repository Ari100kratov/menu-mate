using MenuMate.Common.Application;
using MenuMate.Contracts.ShoppingLists;

namespace MenuMate.Modules.ShoppingLists.Application.SetShoppingListItemState;

internal sealed record SetShoppingListItemStateCommand(
    Guid ItemId,
    ShoppingListItemStateRequest Request)
    : ICommand<ShoppingListResponse>;
