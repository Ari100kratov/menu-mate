using MenuMate.Common.Application;

namespace MenuMate.Modules.ShoppingLists.Application.RemoveShoppingListItem;

internal sealed record RemoveShoppingListItemCommand(Guid ItemId) : ICommand;
