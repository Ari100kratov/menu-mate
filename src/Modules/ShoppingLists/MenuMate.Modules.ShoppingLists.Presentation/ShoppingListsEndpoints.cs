using MenuMate.Common.Application;
using MenuMate.Common.Presentation;
using MenuMate.Contracts.ShoppingLists;
using MenuMate.Modules.ShoppingLists.Application.AddShoppingListItem;
using MenuMate.Modules.ShoppingLists.Application.GenerateShoppingList;
using MenuMate.Modules.ShoppingLists.Application.GetCurrentShoppingList;
using MenuMate.Modules.ShoppingLists.Application.GetMenuShoppingPreview;
using MenuMate.Modules.ShoppingLists.Application.RemoveShoppingListItem;
using MenuMate.Modules.ShoppingLists.Application.SetShoppingListItemState;
using MenuMate.Modules.ShoppingLists.Application.UpdateShoppingListItem;
using MenuMate.SharedKernel;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MenuMate.Modules.ShoppingLists.Presentation;

/// <summary>
/// HTTP-конечные точки единственного списка покупок пользователя.
/// </summary>
public static class ShoppingListsEndpoints
{
    /// <summary>
    /// Регистрирует конечные точки списка покупок.
    /// </summary>
    public static IEndpointRouteBuilder MapShoppingListsEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/shopping-list")
            .WithTags("ShoppingList")
            .RequireAuthorization();

        group.MapGet("/", GetCurrentShoppingListAsync)
            .WithName("GetCurrentShoppingList")
            .Produces<ShoppingListResponse>();

        group.MapGet("/menu-preview", GetMenuShoppingPreviewAsync)
            .WithName("GetMenuShoppingPreview")
            .Produces<MenuShoppingPreviewResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapPut("/from-menu", ReplaceShoppingListFromMenuAsync)
            .WithName("ReplaceShoppingListFromMenu")
            .Accepts<GenerateShoppingListRequest>("application/json")
            .Produces<ShoppingListResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapPost("/items", AddShoppingListItemAsync)
            .WithName("AddShoppingListItem")
            .Accepts<ShoppingListItemRequest>("application/json")
            .Produces<ShoppingListResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapPut("/items/{itemId:guid}", UpdateShoppingListItemAsync)
            .WithName("UpdateShoppingListItem")
            .Accepts<ShoppingListItemRequest>("application/json")
            .Produces<ShoppingListResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPatch("/items/{itemId:guid}/checked", SetShoppingListItemStateAsync)
            .WithName("SetShoppingListItemChecked")
            .Accepts<ShoppingListItemStateRequest>("application/json")
            .Produces<ShoppingListResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/items/{itemId:guid}", RemoveShoppingListItemAsync)
            .WithName("RemoveShoppingListItem")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }

    private static async Task<IResult> GetCurrentShoppingListAsync(
        IQueryHandler<GetCurrentShoppingListQuery, ShoppingListResponse> handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        Result<ShoppingListResponse> result = await handler.Handle(new GetCurrentShoppingListQuery(), cancellationToken);
        return result.ToHttpResult(httpContext);
    }

    private static async Task<IResult> GetMenuShoppingPreviewAsync(
        DateOnly startDate,
        DateOnly endDate,
        IQueryHandler<GetMenuShoppingPreviewQuery, MenuShoppingPreviewResponse> handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        Result<MenuShoppingPreviewResponse> result = await handler.Handle(
            new GetMenuShoppingPreviewQuery(startDate, endDate),
            cancellationToken);
        return result.ToHttpResult(httpContext);
    }

    private static async Task<IResult> ReplaceShoppingListFromMenuAsync(
        GenerateShoppingListRequest request,
        ICommandHandler<GenerateShoppingListCommand, ShoppingListResponse> handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        Result<ShoppingListResponse> result = await handler.Handle(new GenerateShoppingListCommand(request), cancellationToken);
        return result.ToHttpResult(httpContext);
    }

    private static async Task<IResult> AddShoppingListItemAsync(
        ShoppingListItemRequest request,
        ICommandHandler<AddShoppingListItemCommand, ShoppingListResponse> handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        Result<ShoppingListResponse> result = await handler.Handle(new AddShoppingListItemCommand(request), cancellationToken);
        return result.ToHttpResult(httpContext);
    }

    private static async Task<IResult> UpdateShoppingListItemAsync(
        Guid itemId,
        ShoppingListItemRequest request,
        ICommandHandler<UpdateShoppingListItemCommand, ShoppingListResponse> handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        Result<ShoppingListResponse> result = await handler.Handle(
            new UpdateShoppingListItemCommand(itemId, request),
            cancellationToken);
        return result.ToHttpResult(httpContext);
    }

    private static async Task<IResult> SetShoppingListItemStateAsync(
        Guid itemId,
        ShoppingListItemStateRequest request,
        ICommandHandler<SetShoppingListItemStateCommand, ShoppingListResponse> handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        Result<ShoppingListResponse> result = await handler.Handle(
            new SetShoppingListItemStateCommand(itemId, request),
            cancellationToken);
        return result.ToHttpResult(httpContext);
    }

    private static async Task<IResult> RemoveShoppingListItemAsync(
        Guid itemId,
        ICommandHandler<RemoveShoppingListItemCommand> handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        Result result = await handler.Handle(new RemoveShoppingListItemCommand(itemId), cancellationToken);
        return result.ToHttpResult(httpContext);
    }
}
