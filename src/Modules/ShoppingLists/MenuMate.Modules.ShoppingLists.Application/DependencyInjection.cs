using MenuMate.Common.Application;
using MenuMate.Contracts.ShoppingLists;
using MenuMate.Modules.ShoppingLists.Application.AddShoppingListItem;
using MenuMate.Modules.ShoppingLists.Application.DeleteShoppingList;
using MenuMate.Modules.ShoppingLists.Application.GenerateShoppingList;
using MenuMate.Modules.ShoppingLists.Application.GetShoppingListById;
using MenuMate.Modules.ShoppingLists.Application.GetShoppingLists;
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
        services.AddScoped<ICommandHandler<GenerateShoppingListCommand, ShoppingListResponse>, GenerateShoppingListCommandHandler>();
        services.AddScoped<ICommandHandler<AddShoppingListItemCommand, ShoppingListResponse>, AddShoppingListItemCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateShoppingListItemCommand, ShoppingListResponse>, UpdateShoppingListItemCommandHandler>();
        services.AddScoped<ICommandHandler<DeleteShoppingListCommand>, DeleteShoppingListCommandHandler>();
        services.AddScoped<ICommandHandler<RemoveShoppingListItemCommand>, RemoveShoppingListItemCommandHandler>();
        services.AddScoped<ICommandHandler<SetShoppingListItemStateCommand, ShoppingListResponse>, SetShoppingListItemStateCommandHandler>();
        services.AddScoped<IQueryHandler<GetShoppingListByIdQuery, ShoppingListResponse>, GetShoppingListByIdQueryHandler>();
        services.AddScoped<IQueryHandler<GetShoppingListsQuery, IReadOnlyCollection<ShoppingListSummaryResponse>>, GetShoppingListsQueryHandler>();

        return services;
    }
}
