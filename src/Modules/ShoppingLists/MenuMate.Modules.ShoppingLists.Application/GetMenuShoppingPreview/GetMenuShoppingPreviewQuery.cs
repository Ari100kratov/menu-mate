using MenuMate.Common.Application;
using MenuMate.Contracts.ShoppingLists;

namespace MenuMate.Modules.ShoppingLists.Application.GetMenuShoppingPreview;

internal sealed record GetMenuShoppingPreviewQuery(DateOnly StartDate, DateOnly EndDate)
    : IQuery<MenuShoppingPreviewResponse>;
