using MenuMate.Common.Application;
using MenuMate.Common.Presentation;
using MenuMate.Contracts.MenuPlanning;
using MenuMate.Modules.MenuPlanning.Application;
using MenuMate.SharedKernel;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MenuMate.Modules.MenuPlanning.Presentation;

/// <summary>
/// HTTP-конечные точки календаря меню.
/// </summary>
public static class MenuPlanningEndpoints
{
    /// <summary>
    /// Регистрирует конечные точки планирования меню.
    /// </summary>
    public static IEndpointRouteBuilder MapMenuPlanningEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/menu-calendar")
            .WithTags("MenuPlanning")
            .RequireAuthorization();

        group.MapGet("/", GetMenuCalendarAsync)
            .WithName("GetMenuCalendar")
            .Produces<MenuCalendarResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapDelete("/", ClearMenuCalendarAsync)
            .WithName("ClearMenuCalendar")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapPost("/items", AddMenuCalendarItemAsync)
            .WithName("AddMenuCalendarItem")
            .Accepts<CreateMenuCalendarItemRequest>("application/json")
            .Produces<MenuCalendarItemResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/items/{itemId:guid}", UpdateMenuCalendarItemAsync)
            .WithName("UpdateMenuCalendarItem")
            .Accepts<UpdateMenuCalendarItemRequest>("application/json")
            .Produces<MenuCalendarItemResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/items/{itemId:guid}", RemoveMenuCalendarItemAsync)
            .WithName("RemoveMenuCalendarItem")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/meal-slots", GetMealSlotsAsync)
            .WithName("GetMealSlots")
            .Produces<IReadOnlyCollection<MealSlotResponse>>();

        group.MapPost("/meal-slots", CreateMealSlotAsync)
            .WithName("CreateMealSlot")
            .Accepts<CreateMealSlotRequest>("application/json")
            .Produces<IReadOnlyCollection<MealSlotResponse>>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPut("/meal-slots/{mealSlotId:guid}", UpdateMealSlotAsync)
            .WithName("UpdateMealSlot")
            .Accepts<UpdateMealSlotRequest>("application/json")
            .Produces<IReadOnlyCollection<MealSlotResponse>>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapDelete("/meal-slots/{mealSlotId:guid}", DeleteMealSlotAsync)
            .WithName("DeleteMealSlot")
            .Produces<IReadOnlyCollection<MealSlotResponse>>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/meal-slots/order", ReorderMealSlotsAsync)
            .WithName("ReorderMealSlots")
            .Accepts<ReorderMealSlotsRequest>("application/json")
            .Produces<IReadOnlyCollection<MealSlotResponse>>()
            .ProducesProblem(StatusCodes.Status400BadRequest);

        return app;
    }

    private static async Task<IResult> GetMenuCalendarAsync(
        DateOnly startDate,
        DateOnly endDate,
        IQueryHandler<GetMenuCalendarQuery, MenuCalendarResponse> handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        Result<MenuCalendarResponse> result = await handler.Handle(
            new GetMenuCalendarQuery(startDate, endDate),
            cancellationToken);
        return result.ToHttpResult(httpContext);
    }

    private static async Task<IResult> ClearMenuCalendarAsync(
        DateOnly startDate,
        DateOnly endDate,
        ICommandHandler<ClearMenuCalendarCommand> handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        Result result = await handler.Handle(new ClearMenuCalendarCommand(startDate, endDate), cancellationToken);
        return result.ToHttpResult(httpContext);
    }

    private static async Task<IResult> AddMenuCalendarItemAsync(
        CreateMenuCalendarItemRequest request,
        ICommandHandler<AddMenuCalendarItemCommand, MenuCalendarItemResponse> handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        Result<MenuCalendarItemResponse> result = await handler.Handle(
            new AddMenuCalendarItemCommand(request),
            cancellationToken);
        return result.IsSuccess
            ? Results.Created($"/api/menu-calendar/items/{result.Value.Id}", result.Value)
            : result.ToHttpResult(httpContext);
    }

    private static async Task<IResult> UpdateMenuCalendarItemAsync(
        Guid itemId,
        UpdateMenuCalendarItemRequest request,
        ICommandHandler<UpdateMenuCalendarItemCommand, MenuCalendarItemResponse> handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        Result<MenuCalendarItemResponse> result = await handler.Handle(
            new UpdateMenuCalendarItemCommand(itemId, request),
            cancellationToken);
        return result.ToHttpResult(httpContext);
    }

    private static async Task<IResult> RemoveMenuCalendarItemAsync(
        Guid itemId,
        ICommandHandler<RemoveMenuCalendarItemCommand> handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        Result result = await handler.Handle(new RemoveMenuCalendarItemCommand(itemId), cancellationToken);
        return result.ToHttpResult(httpContext);
    }

    private static async Task<IResult> GetMealSlotsAsync(
        IQueryHandler<GetMealSlotsQuery, IReadOnlyCollection<MealSlotResponse>> handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        Result<IReadOnlyCollection<MealSlotResponse>> result =
            await handler.Handle(new GetMealSlotsQuery(), cancellationToken);
        return result.ToHttpResult(httpContext);
    }

    private static async Task<IResult> CreateMealSlotAsync(
        CreateMealSlotRequest request,
        ICommandHandler<CreateMealSlotCommand, IReadOnlyCollection<MealSlotResponse>> handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        Result<IReadOnlyCollection<MealSlotResponse>> result = await handler.Handle(
            new CreateMealSlotCommand(request),
            cancellationToken);
        return result.IsSuccess
            ? Results.Created("/api/menu-calendar/meal-slots", result.Value)
            : result.ToHttpResult(httpContext);
    }

    private static async Task<IResult> UpdateMealSlotAsync(
        Guid mealSlotId,
        UpdateMealSlotRequest request,
        ICommandHandler<UpdateMealSlotCommand, IReadOnlyCollection<MealSlotResponse>> handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        Result<IReadOnlyCollection<MealSlotResponse>> result = await handler.Handle(
            new UpdateMealSlotCommand(mealSlotId, request),
            cancellationToken);
        return result.ToHttpResult(httpContext);
    }

    private static async Task<IResult> DeleteMealSlotAsync(
        Guid mealSlotId,
        ICommandHandler<DeleteMealSlotCommand, IReadOnlyCollection<MealSlotResponse>> handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        Result<IReadOnlyCollection<MealSlotResponse>> result = await handler.Handle(
            new DeleteMealSlotCommand(mealSlotId),
            cancellationToken);
        return result.ToHttpResult(httpContext);
    }

    private static async Task<IResult> ReorderMealSlotsAsync(
        ReorderMealSlotsRequest request,
        ICommandHandler<ReorderMealSlotsCommand, IReadOnlyCollection<MealSlotResponse>> handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        Result<IReadOnlyCollection<MealSlotResponse>> result = await handler.Handle(
            new ReorderMealSlotsCommand(request),
            cancellationToken);
        return result.ToHttpResult(httpContext);
    }
}
