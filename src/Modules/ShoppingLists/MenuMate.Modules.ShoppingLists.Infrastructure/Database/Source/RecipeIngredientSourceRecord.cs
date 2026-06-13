using MenuMate.Modules.ShoppingLists.Domain.Enums;
using MenuMate.SharedKernel.Identifiers;

namespace MenuMate.Modules.ShoppingLists.Infrastructure.Database.Source;

internal sealed class RecipeIngredientSourceRecord
{
    public Guid Id { get; set; }

    public RecipeId RecipeId { get; set; }

    public int Order { get; set; }

    public Guid IngredientId { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public string NormalizedProductName { get; set; } = string.Empty;

    public decimal? Amount { get; set; }

    public ShoppingUnit Unit { get; set; }

    public ShoppingProductCategory Category { get; set; }

    public string? Comment { get; set; }

    public bool IsOptional { get; set; }
}
