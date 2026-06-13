using MenuMate.Common.Application;
using MenuMate.Contracts.ShoppingLists;
using MenuMate.Modules.ShoppingLists.Application.Abstractions;
using MenuMate.Modules.ShoppingLists.Domain.Models;
using MenuMate.Modules.ShoppingLists.Domain.Enums;
using MenuMate.Modules.ShoppingLists.Domain.Services;
using MenuMate.Modules.ShoppingLists.Domain.ValueObjects;
using MenuMate.SharedKernel;

namespace MenuMate.Modules.ShoppingLists.Application.GetMenuShoppingPreview;

internal sealed class GetMenuShoppingPreviewQueryHandler(
    IShoppingListSourceReader sourceReader,
    IUserContext userContext)
    : IQueryHandler<GetMenuShoppingPreviewQuery, MenuShoppingPreviewResponse>
{
    public async Task<Result<MenuShoppingPreviewResponse>> Handle(
        GetMenuShoppingPreviewQuery query,
        CancellationToken cancellationToken)
    {
        if (query.EndDate < query.StartDate)
        {
            return Result.Failure<MenuShoppingPreviewResponse>(ShoppingListApplicationErrors.InvalidDateRange);
        }

        IReadOnlyCollection<ShoppingRecipe> recipes = await sourceReader.GetMenuCalendarRecipesAsync(
            userContext.UserId,
            query.StartDate,
            query.EndDate,
            cancellationToken);

        return new MenuShoppingPreviewResponse(
            query.StartDate,
            query.EndDate,
            [.. recipes.Select(ToResponse)]);
    }

    private static MenuShoppingPreviewRecipeResponse ToResponse(ShoppingRecipe recipe) =>
        new(
            recipe.MenuItemId,
            recipe.Title,
            recipe.BaseServings,
            recipe.TargetServings,
            [.. recipe.Ingredients.Select(ingredient => ToResponse(ingredient, recipe.BaseServings, recipe.TargetServings))]);

    private static MenuShoppingPreviewIngredientResponse ToResponse(
        ShoppingIngredientLine ingredient,
        int baseServings,
        int targetServings)
    {
        decimal factor = baseServings <= 0 ? 1m : (decimal)targetServings / baseServings;
        decimal? scaledAmount = ingredient.Amount.HasValue
            ? Math.Round(ingredient.Amount.Value * factor, 2)
            : null;
        (decimal? amount, ShoppingUnit unit) = ShoppingUnitNormalizer.Normalize(scaledAmount, ingredient.Unit);
        var item = new ShoppingListItem(
            ingredient.ProductId,
            ingredient.Name,
            ingredient.NormalizedName,
            amount,
            unit,
            ingredient.Category,
            ingredient.Comment);

        return new MenuShoppingPreviewIngredientResponse(
            ingredient.LineId == Guid.Empty ? ingredient.ProductId : ingredient.LineId,
            ingredient.ProductId,
            ingredient.Name,
            amount,
            unit.ToString(),
            ingredient.Category.ToString(),
            ShoppingListTextFormatter.FormatAmount(item),
            ingredient.Comment,
            ingredient.IsOptional);
    }
}
