using MenuMate.Common.Application;
using MenuMate.Contracts.MenuPlanning;
using MenuMate.Modules.MenuPlanning.Application.AddMenuPlanItem;
using MenuMate.Modules.MenuPlanning.Application.CreateMenuPlan;
using MenuMate.Modules.MenuPlanning.Application.DeleteMenuPlan;
using MenuMate.Modules.MenuPlanning.Application.GetMenuPlanById;
using MenuMate.Modules.MenuPlanning.Application.GetMenuPlans;
using MenuMate.Modules.MenuPlanning.Application.RemoveMenuPlanItem;
using MenuMate.Modules.MenuPlanning.Application.UpdateMenuPlan;
using MenuMate.Modules.MenuPlanning.Application.UpdateMenuPlanItem;
using Microsoft.Extensions.DependencyInjection;

namespace MenuMate.Modules.MenuPlanning.Application;

/// <summary>
/// Регистрация зависимостей прикладного слоя планирования меню.
/// </summary>
public static class MenuPlanningApplicationDependencyInjection
{
    /// <summary>
    /// Добавляет обработчики сценариев MenuPlanning.
    /// </summary>
    public static IServiceCollection AddMenuPlanningApplication(this IServiceCollection services)
    {
        services.AddScoped<ICommandHandler<CreateMenuPlanCommand, MenuPlanResponse>, CreateMenuPlanCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateMenuPlanCommand, MenuPlanResponse>, UpdateMenuPlanCommandHandler>();
        services.AddScoped<ICommandHandler<DeleteMenuPlanCommand>, DeleteMenuPlanCommandHandler>();
        services.AddScoped<ICommandHandler<AddMenuPlanItemCommand, MenuPlanResponse>, AddMenuPlanItemCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateMenuPlanItemCommand, MenuPlanResponse>, UpdateMenuPlanItemCommandHandler>();
        services.AddScoped<ICommandHandler<RemoveMenuPlanItemCommand>, RemoveMenuPlanItemCommandHandler>();
        services.AddScoped<IQueryHandler<GetMenuPlansQuery, IReadOnlyCollection<MenuPlanResponse>>, GetMenuPlansQueryHandler>();
        services.AddScoped<IQueryHandler<GetMenuPlanByIdQuery, MenuPlanResponse>, GetMenuPlanByIdQueryHandler>();

        return services;
    }
}
