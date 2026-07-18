using MenuMate.Common.Application;
using MenuMate.Common.Presentation;
using MenuMate.Contracts.Recipes;
using MenuMate.Modules.Recipes.Application.CreateRecipe;
using MenuMate.Modules.Recipes.Application.CopyRecipe;
using MenuMate.Modules.Recipes.Application.DeleteRecipe;
using MenuMate.Modules.Recipes.Application.DeleteRecipeImage;
using MenuMate.Modules.Recipes.Application.GetRecipeById;
using MenuMate.Modules.Recipes.Application.GetRecipes;
using MenuMate.Modules.Recipes.Application.SetRecipeFavorite;
using MenuMate.Modules.Recipes.Application.UpdateRecipe;
using MenuMate.Modules.Recipes.Application.UploadRecipeImage;
using MenuMate.SharedKernel;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace MenuMate.Modules.Recipes.Presentation;

/// <summary>
/// HTTP-конечные точки модуля рецептов.
/// </summary>
public static class RecipesEndpoints
{
    /// <summary>
    /// Регистрирует конечные точки рецептов.
    /// </summary>
    public static IEndpointRouteBuilder MapRecipesEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/api/recipes")
            .WithTags("Recipes")
            .RequireAuthorization();

        group.MapGet("/", GetRecipesAsync)
            .WithName("GetRecipes")
            .Produces<IReadOnlyCollection<RecipeListItemResponse>>();

        group.MapGet("/{recipeId:guid}", GetRecipeByIdAsync)
            .WithName("GetRecipeById")
            .Produces<RecipeResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", CreateRecipeAsync)
            .WithName("CreateRecipe")
            .Accepts<CreateRecipeRequest>("application/json")
            .Produces<RecipeResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapPut("/{recipeId:guid}", UpdateRecipeAsync)
            .WithName("UpdateRecipe")
            .Accepts<UpdateRecipeRequest>("application/json")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/{recipeId:guid}", DeleteRecipeAsync)
            .WithName("DeleteRecipe")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/{recipeId:guid}/favorite", MarkRecipeAsFavoriteAsync)
            .WithName("MarkRecipeAsFavorite")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/{recipeId:guid}/copy", CopyRecipeAsync)
            .WithName("CopyRecipe")
            .Accepts<CopyRecipeRequest>("application/json")
            .Produces<RecipeResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/{recipeId:guid}/favorite", UnmarkRecipeAsFavoriteAsync)
            .WithName("UnmarkRecipeAsFavorite")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/{recipeId:guid}/images", UploadRecipeImageAsync)
            .WithName("UploadRecipeImage")
            .Accepts<IFormFile>("multipart/form-data")
            .Produces<RecipeImageResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status502BadGateway)
            .DisableAntiforgery();

        group.MapDelete("/{recipeId:guid}/images/{imageId:guid}", DeleteRecipeImageAsync)
            .WithName("DeleteRecipeImage")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }

    private static async Task<IResult> GetRecipesAsync(
        string? scope,
        string? search,
        Guid[]? tagIds,
        string? category,
        bool? favoritesOnly,
        bool? availableOnly,
        int? page,
        int? pageSize,
        IQueryHandler<GetRecipesQuery, IReadOnlyCollection<RecipeListItemResponse>> handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        Result<IReadOnlyCollection<RecipeListItemResponse>> result = await handler.Handle(
            new GetRecipesQuery(
                scope ?? "library",
                search,
                tagIds ?? [],
                category,
                favoritesOnly ?? false,
                availableOnly ?? false,
                page ?? 1,
                pageSize ?? 20),
            cancellationToken);

        return result.ToHttpResult(httpContext);
    }

    private static async Task<IResult> GetRecipeByIdAsync(
        Guid recipeId,
        Guid? revisionId,
        IQueryHandler<GetRecipeByIdQuery, RecipeResponse> handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        Result<RecipeResponse> result = await handler.Handle(
            new GetRecipeByIdQuery(recipeId, revisionId),
            cancellationToken);
        return result.ToHttpResult(httpContext);
    }

    private static async Task<IResult> CreateRecipeAsync(
        CreateRecipeRequest request,
        ICommandHandler<CreateRecipeCommand, RecipeResponse> handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        Result<RecipeResponse> result = await handler.Handle(new CreateRecipeCommand(request), cancellationToken);
        return result.IsSuccess
            ? Results.Created($"/api/recipes/{result.Value.Id}", result.Value)
            : result.ToHttpResult(httpContext);
    }

    private static async Task<IResult> UpdateRecipeAsync(
        Guid recipeId,
        UpdateRecipeRequest request,
        ICommandHandler<UpdateRecipeCommand> handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        Result result = await handler.Handle(new UpdateRecipeCommand(recipeId, request), cancellationToken);
        return result.ToHttpResult(httpContext);
    }

    private static async Task<IResult> DeleteRecipeAsync(
        Guid recipeId,
        ICommandHandler<DeleteRecipeCommand> handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        Result result = await handler.Handle(new DeleteRecipeCommand(recipeId), cancellationToken);
        return result.ToHttpResult(httpContext);
    }

    private static Task<IResult> MarkRecipeAsFavoriteAsync(
        Guid recipeId,
        Guid? revisionId,
        ICommandHandler<SetRecipeFavoriteCommand> handler,
        HttpContext httpContext,
        CancellationToken cancellationToken) =>
        SetFavoriteAsync(recipeId, isFavorite: true, revisionId, handler, httpContext, cancellationToken);

    private static Task<IResult> UnmarkRecipeAsFavoriteAsync(
        Guid recipeId,
        ICommandHandler<SetRecipeFavoriteCommand> handler,
        HttpContext httpContext,
        CancellationToken cancellationToken) =>
        SetFavoriteAsync(recipeId, isFavorite: false, revisionId: null, handler, httpContext, cancellationToken);

    private static async Task<IResult> SetFavoriteAsync(
        Guid recipeId,
        bool isFavorite,
        Guid? revisionId,
        ICommandHandler<SetRecipeFavoriteCommand> handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        Result result = await handler.Handle(
            new SetRecipeFavoriteCommand(recipeId, isFavorite, revisionId),
            cancellationToken);

        return result.ToHttpResult(httpContext);
    }

    private static async Task<IResult> CopyRecipeAsync(
        Guid recipeId,
        CopyRecipeRequest request,
        ICommandHandler<CopyRecipeCommand, RecipeResponse> handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        Result<RecipeResponse> result = await handler.Handle(
            new CopyRecipeCommand(recipeId, request),
            cancellationToken);
        return result.IsSuccess
            ? Results.Created($"/api/recipes/{result.Value.Id}", result.Value)
            : result.ToHttpResult(httpContext);
    }

    private static async Task<IResult> UploadRecipeImageAsync(
        Guid recipeId,
        [FromForm] IFormFile file,
        [FromForm] string? scope,
        [FromForm] int? stepNumber,
        [FromForm] string? altText,
        ICommandHandler<UploadRecipeImageCommand, RecipeImageResponse> handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        await using Stream content = file.OpenReadStream();
        Result<RecipeImageResponse> result = await handler.Handle(
            new UploadRecipeImageCommand(
                recipeId,
                content,
                file.FileName,
                file.ContentType,
                file.Length,
                scope,
                stepNumber,
                altText),
            cancellationToken);

        return result.IsSuccess
            ? Results.Created($"/api/recipes/{recipeId}/images/{result.Value.Id}", result.Value)
            : result.ToHttpResult(httpContext);
    }

    private static async Task<IResult> DeleteRecipeImageAsync(
        Guid recipeId,
        Guid imageId,
        ICommandHandler<DeleteRecipeImageCommand> handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        Result result = await handler.Handle(new DeleteRecipeImageCommand(recipeId, imageId), cancellationToken);
        return result.ToHttpResult(httpContext);
    }
}
