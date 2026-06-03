using MenuMate.Common.Application;
using MenuMate.Contracts.ShoppingLists;

namespace MenuMate.Modules.ShoppingLists.Application.UpdateShoppingListItem;

internal sealed record UpdateShoppingListItemCommand(
    Guid ShoppingListId,
    Guid ItemId,
    ShoppingListItemRequest Request)
    : ICommand<ShoppingListResponse>;
