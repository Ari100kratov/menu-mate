using MenuMate.Common.Application;
using MenuMate.Common.Presentation;
using MenuMate.Contracts.RecipeImports;
using MenuMate.Contracts.Recipes;
using MenuMate.Modules.RecipeImports.Application;
using MenuMate.Modules.RecipeImports.Application.ConfirmRecipeImportDraft;
using MenuMate.Modules.RecipeImports.Application.CreateRecipeImportDraft;
using MenuMate.Modules.RecipeImports.Application.DeleteRecipeImportDraft;
using MenuMate.Modules.RecipeImports.Application.GetRecipeImportDraft;
using MenuMate.Modules.RecipeImports.Application.GetRecipeImportDrafts;
using MenuMate.Modules.RecipeImports.Application.GetRecipeImportSourceImage;
using MenuMate.Modules.RecipeImports.Application.GenerateRecipeCoverImage;
using MenuMate.Modules.RecipeImports.Application.Generation;
using MenuMate.Modules.RecipeImports.Application.UpdateRecipeImportDraft;
using MenuMate.SharedKernel;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace MenuMate.Modules.RecipeImports.Presentation;

/// <summary>
/// HTTP-конечные точки черновиков импорта рецептов.
/// </summary>
public static class RecipeImportsEndpoints
{
    /// <summary>Регистрирует конечные точки модуля Imports.</summary>
    public static IEndpointRouteBuilder MapRecipeImportsEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/imports/recipe-drafts")
            .WithTags("RecipeImports")
            .RequireAuthorization();

        group.MapGet("/", GetDraftsAsync)
            .WithName("GetRecipeImportDrafts")
            .Produces<IReadOnlyCollection<RecipeImportDraftListItemResponse>>();

        group.MapGet("/{draftId:guid}", GetDraftAsync)
            .WithName("GetRecipeImportDraft")
            .Produces<RecipeImportDraftResponse>()
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/{draftId:guid}/source-images/{sourceImageIndex:int:min(0)}/content", GetSourceImageAsync)
            .WithName("GetRecipeImportSourceImage")
            .Produces(StatusCodes.Status200OK, contentType: "application/octet-stream")
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", CreateDraftAsync)
            .WithName("CreateRecipeImportDraft")
            .Accepts<IFormFileCollection>("multipart/form-data")
            .Produces<RecipeImportDraftResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status502BadGateway)
            .DisableAntiforgery();

        group.MapPut("/{draftId:guid}", UpdateDraftAsync)
            .WithName("UpdateRecipeImportDraft")
            .Accepts<UpdateRecipeImportDraftRequest>("application/json")
            .Produces<RecipeImportDraftResponse>()
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("/{draftId:guid}/confirm", ConfirmDraftAsync)
            .WithName("ConfirmRecipeImportDraft")
            .Accepts<CreateRecipeRequest>("application/json")
            .Produces<RecipeResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/{draftId:guid}", DeleteDraftAsync)
            .WithName("DeleteRecipeImportDraft")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        app.MapPost("/api/recipe-images/generate-cover", GenerateCoverAsync)
            .WithName("GenerateRecipeCoverImage")
            .WithTags("RecipeImports")
            .RequireAuthorization()
            .Accepts<CreateRecipeRequest>("application/json")
            .Produces(StatusCodes.Status200OK, contentType: "image/jpeg")
            .ProducesProblem(StatusCodes.Status502BadGateway);

        return app;
    }

    private static async Task<IResult> GetDraftsAsync(
        IQueryHandler<GetRecipeImportDraftsQuery, IReadOnlyCollection<RecipeImportDraftListItemResponse>> handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        Result<IReadOnlyCollection<RecipeImportDraftListItemResponse>> result = await handler.Handle(
            new GetRecipeImportDraftsQuery(),
            cancellationToken);
        return result.ToHttpResult(httpContext);
    }

    private static async Task<IResult> GetDraftAsync(
        Guid draftId,
        IQueryHandler<GetRecipeImportDraftQuery, RecipeImportDraftResponse> handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        Result<RecipeImportDraftResponse> result = await handler.Handle(
            new GetRecipeImportDraftQuery(draftId),
            cancellationToken);
        return result.ToHttpResult(httpContext);
    }

    private static async Task<IResult> GetSourceImageAsync(
        Guid draftId,
        int sourceImageIndex,
        IQueryHandler<GetRecipeImportSourceImageQuery, RecipeImportSourceImageContent> handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        Result<RecipeImportSourceImageContent> result = await handler.Handle(
            new GetRecipeImportSourceImageQuery(draftId, sourceImageIndex),
            cancellationToken);
        if (result.IsFailure)
        {
            return result.ToHttpResult(httpContext);
        }

        httpContext.Response.Headers.CacheControl = "private, no-store";
        return Results.Stream(
            result.Value.Content,
            result.Value.ContentType,
            enableRangeProcessing: false);
    }

    private static async Task<IResult> CreateDraftAsync(
        ICommandHandler<CreateRecipeImportDraftCommand, RecipeImportDraftResponse> handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (!httpContext.Request.HasFormContentType)
        {
            return Result.Failure<RecipeImportDraftResponse>(ImportApplicationErrors.ImagesRequired)
                .ToHttpResult(httpContext);
        }

        IFormFileCollection files = (await httpContext.Request.ReadFormAsync(cancellationToken)).Files;
        var streams = new List<Stream>(files.Count);
        streams.AddRange(files.Select(file => file.OpenReadStream()));
        try
        {
            Result<RecipeImportDraftResponse> result = await handler.Handle(
                new CreateRecipeImportDraftCommand(
                    files.Zip(
                        streams,
                        (file, stream) => new RecipeImportSourceFile(
                            stream,
                            file.FileName,
                            file.ContentType,
                            file.Length))
                        .ToArray()),
                cancellationToken);
            return result.IsSuccess
                ? Results.Created($"/api/imports/recipe-drafts/{result.Value.Id}", result.Value)
                : result.ToHttpResult(httpContext);
        }
        finally
        {
            foreach (Stream stream in streams)
            {
                await stream.DisposeAsync();
            }
        }
    }

    private static async Task<IResult> UpdateDraftAsync(
        Guid draftId,
        UpdateRecipeImportDraftRequest request,
        ICommandHandler<UpdateRecipeImportDraftCommand, RecipeImportDraftResponse> handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        Result<RecipeImportDraftResponse> result = await handler.Handle(
            new UpdateRecipeImportDraftCommand(draftId, request),
            cancellationToken);
        return result.ToHttpResult(httpContext);
    }

    private static async Task<IResult> ConfirmDraftAsync(
        Guid draftId,
        CreateRecipeRequest request,
        ICommandHandler<ConfirmRecipeImportDraftCommand, RecipeResponse> handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        Result<RecipeResponse> result = await handler.Handle(
            new ConfirmRecipeImportDraftCommand(draftId, request),
            cancellationToken);
        return result.IsSuccess
            ? Results.Created($"/api/recipes/{result.Value.Id}", result.Value)
            : result.ToHttpResult(httpContext);
    }

    private static async Task<IResult> DeleteDraftAsync(
        Guid draftId,
        ICommandHandler<DeleteRecipeImportDraftCommand> handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        Result result = await handler.Handle(
            new DeleteRecipeImportDraftCommand(draftId),
            cancellationToken);
        return result.ToHttpResult(httpContext);
    }

    private static async Task<IResult> GenerateCoverAsync(
        CreateRecipeRequest request,
        ICommandHandler<GenerateRecipeCoverImageCommand, GeneratedRecipeCoverImage> handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        Result<GeneratedRecipeCoverImage> result = await handler.Handle(
            new GenerateRecipeCoverImageCommand(request),
            cancellationToken);
        return result.IsSuccess
            ? Results.File(
                result.Value.Content.ToArray(),
                result.Value.ContentType,
                result.Value.FileName)
            : result.ToHttpResult(httpContext);
    }
}
