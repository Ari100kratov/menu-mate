using MenuMate.Common.Application;
using MenuMate.Contracts.ShoppingLists;

namespace MenuMate.Modules.ShoppingLists.Application.AddShoppingListItem;

internal sealed record AddShoppingListItemCommand(ShoppingListItemRequest Request)
    : ICommand<ShoppingListResponse>;
