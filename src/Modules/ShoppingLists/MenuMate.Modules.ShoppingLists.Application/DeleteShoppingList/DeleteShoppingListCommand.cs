using MenuMate.Common.Application;

namespace MenuMate.Modules.ShoppingLists.Application.DeleteShoppingList;

internal sealed record DeleteShoppingListCommand(Guid ShoppingListId) : ICommand;
