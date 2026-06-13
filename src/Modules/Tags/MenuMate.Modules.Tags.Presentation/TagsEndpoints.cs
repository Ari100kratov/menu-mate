using MenuMate.Common.Application;
using MenuMate.Common.Presentation;
using MenuMate.Contracts.Tags;
using MenuMate.Modules.Tags.Application.ConfirmTag;
using MenuMate.Modules.Tags.Application.CreateTag;
using MenuMate.Modules.Tags.Application.GetTags;
using MenuMate.Modules.Tags.Application.HideTag;
using MenuMate.SharedKernel;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MenuMate.Modules.Tags.Presentation;

/// <summary>
/// HTTP-конечные точки модуля тегов.
/// </summary>
public static class TagsEndpoints
{
    /// <summary>
    /// Регистрирует конечные точки тегов.
    /// </summary>
    public static IEndpointRouteBuilder MapTagsEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/tags")
            .WithTags("Tags")
            .RequireAuthorization();

        group.MapGet("/", GetTagsAsync)
            .WithName("GetTags")
            .Produces<IReadOnlyCollection<TagResponse>>();

        group.MapPost("/", CreateTagAsync)
            .WithName("CreateTag")
            .Accepts<CreateTagRequest>("application/json")
            .Produces<TagResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapPost("/{tagId:guid}/confirm", ConfirmTagAsync)
            .WithName("ConfirmTag")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/{tagId:guid}", HideTagAsync)
            .WithName("HideTag")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }

    private static async Task<IResult> GetTagsAsync(
        string? search,
        bool? includeHidden,
        IQueryHandler<GetTagsQuery, IReadOnlyCollection<TagResponse>> handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        Result<IReadOnlyCollection<TagResponse>> result = await handler.Handle(
            new GetTagsQuery(search, includeHidden ?? false),
            cancellationToken);

        return result.ToHttpResult(httpContext);
    }

    private static async Task<IResult> CreateTagAsync(
        CreateTagRequest request,
        ICommandHandler<CreateTagCommand, TagResponse> handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        Result<TagResponse> result = await handler.Handle(new CreateTagCommand(request), cancellationToken);

        return result.IsSuccess
            ? Results.Created($"/api/tags/{result.Value.Id}", result.Value)
            : result.ToHttpResult(httpContext);
    }

    private static async Task<IResult> ConfirmTagAsync(
        Guid tagId,
        ICommandHandler<ConfirmTagCommand> handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        Result result = await handler.Handle(new ConfirmTagCommand(tagId), cancellationToken);
        return result.ToHttpResult(httpContext);
    }

    private static async Task<IResult> HideTagAsync(
        Guid tagId,
        ICommandHandler<HideTagCommand> handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        Result result = await handler.Handle(new HideTagCommand(tagId), cancellationToken);
        return result.ToHttpResult(httpContext);
    }
}
