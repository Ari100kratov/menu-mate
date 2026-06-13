using MenuMate.Common.Application;
using MenuMate.Contracts.MenuPlanning;
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
        services.AddScoped<IQueryHandler<GetMenuCalendarQuery, MenuCalendarResponse>, GetMenuCalendarQueryHandler>();
        services.AddScoped<IQueryHandler<GetMealSlotsQuery, IReadOnlyCollection<MealSlotResponse>>, GetMealSlotsQueryHandler>();
        services.AddScoped<ICommandHandler<AddMenuCalendarItemCommand, MenuCalendarItemResponse>, AddMenuCalendarItemCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateMenuCalendarItemCommand, MenuCalendarItemResponse>, UpdateMenuCalendarItemCommandHandler>();
        services.AddScoped<ICommandHandler<RemoveMenuCalendarItemCommand>, RemoveMenuCalendarItemCommandHandler>();
        services.AddScoped<ICommandHandler<ClearMenuCalendarCommand>, ClearMenuCalendarCommandHandler>();
        services.AddScoped<ICommandHandler<CreateMealSlotCommand, IReadOnlyCollection<MealSlotResponse>>, CreateMealSlotCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateMealSlotCommand, IReadOnlyCollection<MealSlotResponse>>, UpdateMealSlotCommandHandler>();
        services.AddScoped<ICommandHandler<DeleteMealSlotCommand, IReadOnlyCollection<MealSlotResponse>>, DeleteMealSlotCommandHandler>();
        services.AddScoped<ICommandHandler<ReorderMealSlotsCommand, IReadOnlyCollection<MealSlotResponse>>, ReorderMealSlotsCommandHandler>();
        services.AddScoped<IUserRegistrationInitializer, DefaultMealSlotRegistrationInitializer>();

        return services;
    }
}
