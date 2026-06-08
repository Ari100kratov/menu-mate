using MenuMate.Modules.Recipes.Domain.Enums;
using MenuMate.Modules.Recipes.Domain.Models;
using MenuMate.Modules.Recipes.Domain.ValueObjects;
using MenuMate.SharedKernel;

namespace MenuMate.Modules.Recipes.Infrastructure.Database.Entities;

internal sealed class RecipeIngredientRecord
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public Guid RecipeId { get; set; }

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

    public static RecipeIngredientRecord FromDomain(RecipeIngredient ingredient, int index) =>
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

    public RecipeIngredient ToDomain()
    {
        Result<IngredientName> name = IngredientName.Create(ProductName);
        if (name.IsFailure)
        {
            throw new DomainException(name.Error);
        }

        Result<IngredientQuantity> quantity = QuantityKind switch
        {
            IngredientQuantityKind.ToTaste => IngredientQuantity.ToTaste(),
            IngredientQuantityKind.Approximate => IngredientQuantity.Approximate(Amount.GetValueOrDefault(), Unit),
            _ => IngredientQuantity.Exact(Amount.GetValueOrDefault(), Unit)
        };

        if (quantity.IsFailure)
        {
            throw new DomainException(quantity.Error);
        }

        return new RecipeIngredient(IngredientId, name.Value, quantity.Value, Category, Comment, IsOptional);
    }
}
