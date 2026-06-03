using MenuMate.Common.Application;
using MenuMate.Contracts.ShoppingLists;

namespace MenuMate.Modules.ShoppingLists.Application.GetShoppingListById;

internal sealed record GetShoppingListByIdQuery(Guid ShoppingListId) : IQuery<ShoppingListResponse>;
