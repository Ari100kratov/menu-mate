using MenuMate.Common.Application;
using MenuMate.Common.Presentation;
using MenuMate.Contracts.MenuPlanning;
using MenuMate.Modules.MenuPlanning.Application.AddMenuPlanItem;
using MenuMate.Modules.MenuPlanning.Application.CreateMenuPlan;
using MenuMate.Modules.MenuPlanning.Application.DeleteMenuPlan;
using MenuMate.Modules.MenuPlanning.Application.GetMenuPlanById;
using MenuMate.Modules.MenuPlanning.Application.GetMenuPlans;
using MenuMate.Modules.MenuPlanning.Application.RemoveMenuPlanItem;
using MenuMate.Modules.MenuPlanning.Application.UpdateMenuPlan;
using MenuMate.Modules.MenuPlanning.Application.UpdateMenuPlanItem;
using MenuMate.SharedKernel;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MenuMate.Modules.MenuPlanning.Presentation;

/// <summary>
/// HTTP-конечные точки модуля планирования меню.
/// </summary>
public static class MenuPlanningEndpoints
{
    /// <summary>
    /// Регистрирует конечные точки планирования меню.
    /// </summary>
    public static IEndpointRouteBuilder MapMenuPlanningEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/menu-plans")
            .WithTags("MenuPlanning")
            .RequireAuthorization();

        group.MapGet("/", GetMenuPlansAsync)
            .WithName("GetMenuPlans")
            .Produces<IReadOnlyCollection<MenuPlanResponse>>();

        group.MapGet("/{menuPlanId:guid}", GetMenuPlanByIdAsync)
            .WithName("GetMenuPlanById")
            .Produces<MenuPlanResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", CreateMenuPlanAsync)
            .WithName("CreateMenuPlan")
            .Accepts<CreateMenuPlanRequest>("application/json")
            .Produces<MenuPlanResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapPut("/{menuPlanId:guid}", UpdateMenuPlanAsync)
            .WithName("UpdateMenuPlan")
            .Accepts<UpdateMenuPlanRequest>("application/json")
            .Produces<MenuPlanResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/{menuPlanId:guid}", DeleteMenuPlanAsync)
            .WithName("DeleteMenuPlan")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/{menuPlanId:guid}/items", AddMenuPlanItemAsync)
            .WithName("AddMenuPlanItem")
            .Accepts<CreateMenuPlanItemRequest>("application/json")
            .Produces<MenuPlanResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/{menuPlanId:guid}/items/{itemId:guid}", UpdateMenuPlanItemAsync)
            .WithName("UpdateMenuPlanItem")
            .Accepts<UpdateMenuPlanItemRequest>("application/json")
            .Produces<MenuPlanResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/{menuPlanId:guid}/items/{itemId:guid}", RemoveMenuPlanItemAsync)
            .WithName("RemoveMenuPlanItem")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }

    private static async Task<IResult> GetMenuPlansAsync(
        IQueryHandler<GetMenuPlansQuery, IReadOnlyCollection<MenuPlanResponse>> handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        Result<IReadOnlyCollection<MenuPlanResponse>> result = await handler.Handle(
            new GetMenuPlansQuery(),
            cancellationToken);

        return result.ToHttpResult(httpContext);
    }

    private static async Task<IResult> GetMenuPlanByIdAsync(
        Guid menuPlanId,
        IQueryHandler<GetMenuPlanByIdQuery, MenuPlanResponse> handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        Result<MenuPlanResponse> result = await handler.Handle(
            new GetMenuPlanByIdQuery(menuPlanId),
            cancellationToken);

        return result.ToHttpResult(httpContext);
    }

    private static async Task<IResult> CreateMenuPlanAsync(
        CreateMenuPlanRequest request,
        ICommandHandler<CreateMenuPlanCommand, MenuPlanResponse> handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        Result<MenuPlanResponse> result = await handler.Handle(new CreateMenuPlanCommand(request), cancellationToken);

        return result.IsSuccess
            ? Results.Created($"/api/menu-plans/{result.Value.Id}", result.Value)
            : result.ToHttpResult(httpContext);
    }

    private static async Task<IResult> UpdateMenuPlanAsync(
        Guid menuPlanId,
        UpdateMenuPlanRequest request,
        ICommandHandler<UpdateMenuPlanCommand, MenuPlanResponse> handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        Result<MenuPlanResponse> result = await handler.Handle(
            new UpdateMenuPlanCommand(menuPlanId, request),
            cancellationToken);

        return result.ToHttpResult(httpContext);
    }

    private static async Task<IResult> DeleteMenuPlanAsync(
        Guid menuPlanId,
        ICommandHandler<DeleteMenuPlanCommand> handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        Result result = await handler.Handle(new DeleteMenuPlanCommand(menuPlanId), cancellationToken);
        return result.ToHttpResult(httpContext);
    }

    private static async Task<IResult> AddMenuPlanItemAsync(
        Guid menuPlanId,
        CreateMenuPlanItemRequest request,
        ICommandHandler<AddMenuPlanItemCommand, MenuPlanResponse> handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        Result<MenuPlanResponse> result = await handler.Handle(
            new AddMenuPlanItemCommand(menuPlanId, request),
            cancellationToken);

        return result.ToHttpResult(httpContext);
    }

    private static async Task<IResult> UpdateMenuPlanItemAsync(
        Guid menuPlanId,
        Guid itemId,
        UpdateMenuPlanItemRequest request,
        ICommandHandler<UpdateMenuPlanItemCommand, MenuPlanResponse> handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        Result<MenuPlanResponse> result = await handler.Handle(
            new UpdateMenuPlanItemCommand(menuPlanId, itemId, request),
            cancellationToken);

        return result.ToHttpResult(httpContext);
    }

    private static async Task<IResult> RemoveMenuPlanItemAsync(
        Guid menuPlanId,
        Guid itemId,
        ICommandHandler<RemoveMenuPlanItemCommand> handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        Result result = await handler.Handle(new RemoveMenuPlanItemCommand(menuPlanId, itemId), cancellationToken);
        return result.ToHttpResult(httpContext);
    }
}
