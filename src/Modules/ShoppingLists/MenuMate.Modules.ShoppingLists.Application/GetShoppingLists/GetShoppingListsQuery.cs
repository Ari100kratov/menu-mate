using MenuMate.Common.Application;
using MenuMate.Contracts.ShoppingLists;

namespace MenuMate.Modules.ShoppingLists.Application.GetShoppingLists;

internal sealed record GetShoppingListsQuery : IQuery<IReadOnlyCollection<ShoppingListSummaryResponse>>;
