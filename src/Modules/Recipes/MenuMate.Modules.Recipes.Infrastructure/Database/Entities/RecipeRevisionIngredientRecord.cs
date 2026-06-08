using MenuMate.Modules.Recipes.Domain.Enums;
using MenuMate.Modules.Recipes.Domain.Models;

namespace MenuMate.Modules.Recipes.Infrastructure.Database.Entities;

internal sealed class RecipeRevisionIngredientRecord
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public Guid RecipeRevisionId { get; set; }

    public int Order { get; set; }

    public Guid IngredientId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public string NormalizedProductName { get; set; } = string.Empty;

    public decimal? Amount { get; set; }

    public MeasurementUnit Unit { get; set; }

    public IngredientQuantityKind QuantityKind { get; set; }

    public ProductCategory Category { get; set; }

    public string? Comment { get; set; }

    public bool IsOptional { get; set; }

    public static RecipeRevisionIngredientRecord FromDomain(RecipeIngredient ingredient, int index) =>
        new()
        {
            Order = index,
            IngredientId = ingredient.IngredientId ?? throw new InvalidOperationException(
                "Recipe ingredient must reference a catalog product before persistence."),
            ProductName = ingredient.Name.Value,
            NormalizedProductName = ingredient.Name.NormalizedValue,
            Amount = ingredient.Quantity.Amount,
            Unit = ingredient.Quantity.Unit,
            QuantityKind = ingredient.Quantity.Kind,
            Category = ingredient.Category,
            Comment = ingredient.Comment,
            IsOptional = ingredient.IsOptional
        };
}
