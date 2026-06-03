using MenuMate.Common.Application;
using MenuMate.Contracts.ShoppingLists;

namespace MenuMate.Modules.ShoppingLists.Application.GenerateShoppingList;

internal sealed record GenerateShoppingListCommand(GenerateShoppingListRequest Request)
    : ICommand<ShoppingListResponse>;
