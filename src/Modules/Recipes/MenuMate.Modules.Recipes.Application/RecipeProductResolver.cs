using MenuMate.Common.Application.Products;
using MenuMate.Modules.Recipes.Domain.Enums;
using MenuMate.Modules.Recipes.Domain.Models;
using MenuMate.Modules.Recipes.Domain.ValueObjects;
using MenuMate.SharedKernel;

namespace MenuMate.Modules.Recipes.Application;

internal sealed class RecipeProductResolver(IProductCatalog productCatalog)
{
    public async Task<Result<IReadOnlyCollection<RecipeIngredient>>> ResolveAsync(
        IReadOnlyCollection<RecipeIngredient> ingredients,
        CancellationToken cancellationToken)
    {
        var resolved = new List<RecipeIngredient>(ingredients.Count);

        foreach (RecipeIngredient ingredient in ingredients)
        {
            ProductCatalogItem product = await productCatalog.ResolveAsync(
                ingredient.IngredientId,
                ingredient.Name.Value,
                ingredient.Category.ToString(),
                cancellationToken);

            Result<IngredientName> name = IngredientName.Create(product.Name);
            if (name.IsFailure)
            {
                return Result.Failure<IReadOnlyCollection<RecipeIngredient>>(name.Error);
            }

            if (!Enum.TryParse(product.Category, ignoreCase: true, out ProductCategory category))
            {
                return Result.Failure<IReadOnlyCollection<RecipeIngredient>>(AppError.Validation(
                    "Recipes.InvalidCatalogProductCategory",
                    "Категория продукта из каталога указана в неизвестном формате."));
            }

            resolved.Add(ingredient with
            {
                IngredientId = product.Id,
                Name = name.Value,
                Category = category
            });
        }

        return resolved;
    }
}
