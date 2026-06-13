using MenuMate.Common.Application;
using MenuMate.Contracts.ShoppingLists;
using MenuMate.Modules.ShoppingLists.Application.AddShoppingListItem;
using MenuMate.Modules.ShoppingLists.Application.GenerateShoppingList;
using MenuMate.Modules.ShoppingLists.Application.GetCurrentShoppingList;
using MenuMate.Modules.ShoppingLists.Application.GetMenuShoppingPreview;
using MenuMate.Modules.ShoppingLists.Application.RemoveShoppingListItem;
using MenuMate.Modules.ShoppingLists.Application.SetShoppingListItemState;
using MenuMate.Modules.ShoppingLists.Application.UpdateShoppingListItem;
using Microsoft.Extensions.DependencyInjection;

namespace MenuMate.Modules.ShoppingLists.Application;

/// <summary>
/// Регистрация прикладного слоя модуля списков покупок.
/// </summary>
public static class ShoppingListsApplicationDependencyInjection
{
    /// <summary>
    /// Добавляет обработчики use case-ов ShoppingLists.
    /// </summary>
    public static IServiceCollection AddShoppingListsApplication(this IServiceCollection services)
    {
        services.AddScoped<ShoppingProductResolver>();
        services.AddScoped<ICommandHandler<GenerateShoppingListCommand, ShoppingListResponse>, GenerateShoppingListCommandHandler>();
        services.AddScoped<ICommandHandler<AddShoppingListItemCommand, ShoppingListResponse>, AddShoppingListItemCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateShoppingListItemCommand, ShoppingListResponse>, UpdateShoppingListItemCommandHandler>();
        services.AddScoped<ICommandHandler<RemoveShoppingListItemCommand>, RemoveShoppingListItemCommandHandler>();
        services.AddScoped<ICommandHandler<SetShoppingListItemStateCommand, ShoppingListResponse>, SetShoppingListItemStateCommandHandler>();
        services.AddScoped<IQueryHandler<GetCurrentShoppingListQuery, ShoppingListResponse>, GetCurrentShoppingListQueryHandler>();
        services.AddScoped<IQueryHandler<GetMenuShoppingPreviewQuery, MenuShoppingPreviewResponse>, GetMenuShoppingPreviewQueryHandler>();

        return services;
    }
}
