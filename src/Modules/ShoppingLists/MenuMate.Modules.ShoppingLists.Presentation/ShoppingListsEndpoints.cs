using MenuMate.Common.Application;
using MenuMate.Common.Presentation;
using MenuMate.Contracts.ShoppingLists;
using MenuMate.Modules.ShoppingLists.Application.AddShoppingListItem;
using MenuMate.Modules.ShoppingLists.Application.DeleteShoppingList;
using MenuMate.Modules.ShoppingLists.Application.GenerateShoppingList;
using MenuMate.Modules.ShoppingLists.Application.GetShoppingListById;
using MenuMate.Modules.ShoppingLists.Application.GetShoppingLists;
using MenuMate.Modules.ShoppingLists.Application.RemoveShoppingListItem;
using MenuMate.Modules.ShoppingLists.Application.SetShoppingListItemState;
using MenuMate.Modules.ShoppingLists.Application.UpdateShoppingListItem;
using MenuMate.SharedKernel;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MenuMate.Modules.ShoppingLists.Presentation;

/// <summary>
/// HTTP-конечные точки модуля списков покупок.
/// </summary>
public static class ShoppingListsEndpoints
{
    /// <summary>
    /// Регистрирует конечные точки списков покупок.
    /// </summary>
    public static IEndpointRouteBuilder MapShoppingListsEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/shopping-lists")
            .WithTags("ShoppingLists")
            .RequireAuthorization();

        group.MapGet("/", GetShoppingListsAsync)
            .WithName("GetShoppingLists")
            .Produces<IReadOnlyCollection<ShoppingListSummaryResponse>>();

        group.MapGet("/{shoppingListId:guid}", GetShoppingListByIdAsync)
            .WithName("GetShoppingListById")
            .Produces<ShoppingListResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", GenerateShoppingListAsync)
            .WithName("GenerateShoppingList")
            .Accepts<GenerateShoppingListRequest>("application/json")
            .Produces<ShoppingListResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/{shoppingListId:guid}", DeleteShoppingListAsync)
            .WithName("DeleteShoppingList")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/{shoppingListId:guid}/items", AddShoppingListItemAsync)
            .WithName("AddShoppingListItem")
            .Accepts<ShoppingListItemRequest>("application/json")
            .Produces<ShoppingListResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/{shoppingListId:guid}/items/{itemId:guid}", UpdateShoppingListItemAsync)
            .WithName("UpdateShoppingListItem")
            .Accepts<ShoppingListItemRequest>("application/json")
            .Produces<ShoppingListResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPatch("/{shoppingListId:guid}/items/{itemId:guid}/state", SetShoppingListItemStateAsync)
            .WithName("SetShoppingListItemState")
            .Accepts<ShoppingListItemStateRequest>("application/json")
            .Produces<ShoppingListResponse>()
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/{shoppingListId:guid}/items/{itemId:guid}", RemoveShoppingListItemAsync)
            .WithName("RemoveShoppingListItem")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }

    private static async Task<IResult> GetShoppingListsAsync(
        IQueryHandler<GetShoppingListsQuery, IReadOnlyCollection<ShoppingListSummaryResponse>> handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        Result<IReadOnlyCollection<ShoppingListSummaryResponse>> result = await handler.Handle(
            new GetShoppingListsQuery(),
            cancellationToken);

        return result.ToHttpResult(httpContext);
    }

    private static async Task<IResult> GetShoppingListByIdAsync(
        Guid shoppingListId,
        IQueryHandler<GetShoppingListByIdQuery, ShoppingListResponse> handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        Result<ShoppingListResponse> result = await handler.Handle(
            new GetShoppingListByIdQuery(shoppingListId),
            cancellationToken);

        return result.ToHttpResult(httpContext);
    }

    private static async Task<IResult> GenerateShoppingListAsync(
        GenerateShoppingListRequest request,
        ICommandHandler<GenerateShoppingListCommand, ShoppingListResponse> handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        Result<ShoppingListResponse> result = await handler.Handle(
            new GenerateShoppingListCommand(request),
            cancellationToken);

        return result.IsSuccess
            ? Results.Created($"/api/shopping-lists/{result.Value.Id}", result.Value)
            : result.ToHttpResult(httpContext);
    }

    private static async Task<IResult> DeleteShoppingListAsync(
        Guid shoppingListId,
        ICommandHandler<DeleteShoppingListCommand> handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        Result result = await handler.Handle(new DeleteShoppingListCommand(shoppingListId), cancellationToken);
        return result.ToHttpResult(httpContext);
    }

    private static async Task<IResult> AddShoppingListItemAsync(
        Guid shoppingListId,
        ShoppingListItemRequest request,
        ICommandHandler<AddShoppingListItemCommand, ShoppingListResponse> handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        Result<ShoppingListResponse> result = await handler.Handle(
            new AddShoppingListItemCommand(shoppingListId, request),
            cancellationToken);

        return result.ToHttpResult(httpContext);
    }

    private static async Task<IResult> UpdateShoppingListItemAsync(
        Guid shoppingListId,
        Guid itemId,
        ShoppingListItemRequest request,
        ICommandHandler<UpdateShoppingListItemCommand, ShoppingListResponse> handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        Result<ShoppingListResponse> result = await handler.Handle(
            new UpdateShoppingListItemCommand(shoppingListId, itemId, request),
            cancellationToken);

        return result.ToHttpResult(httpContext);
    }

    private static async Task<IResult> SetShoppingListItemStateAsync(
        Guid shoppingListId,
        Guid itemId,
        ShoppingListItemStateRequest request,
        ICommandHandler<SetShoppingListItemStateCommand, ShoppingListResponse> handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        Result<ShoppingListResponse> result = await handler.Handle(
            new SetShoppingListItemStateCommand(shoppingListId, itemId, request),
            cancellationToken);

        return result.ToHttpResult(httpContext);
    }

    private static async Task<IResult> RemoveShoppingListItemAsync(
        Guid shoppingListId,
        Guid itemId,
        ICommandHandler<RemoveShoppingListItemCommand> handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        Result result = await handler.Handle(
            new RemoveShoppingListItemCommand(shoppingListId, itemId),
            cancellationToken);

        return result.ToHttpResult(httpContext);
    }
}
