using MenuMate.Common.Application;
using MenuMate.Contracts.ShoppingLists;

namespace MenuMate.Modules.ShoppingLists.Application.GetCurrentShoppingList;

internal sealed record GetCurrentShoppingListQuery : IQuery<ShoppingListResponse>;
