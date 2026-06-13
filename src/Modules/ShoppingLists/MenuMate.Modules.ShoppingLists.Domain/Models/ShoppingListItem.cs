using MenuMate.Modules.ShoppingLists.Domain.Enums;

namespace MenuMate.Modules.ShoppingLists.Domain.Models;

/// <summary>
/// Позиция списка покупок.
/// </summary>
public sealed record ShoppingListItem
{
    /// <summary>
    /// Инициализирует новую позицию списка покупок.
    /// </summary>
    public ShoppingListItem(
        Guid productId,
        string name,
        string normalizedName,
        decimal? amount,
        ShoppingUnit unit,
        ShoppingProductCategory category,
        string? comment,
        bool isPurchased = false)
    {
        ProductId = productId;
        Name = name;
        NormalizedName = normalizedName;
        Amount = amount;
        Unit = unit;
        Category = category;
        Comment = comment;
        IsPurchased = isPurchased;
    }

    /// <summary>
    /// Идентификатор продукта общего каталога.
    /// </summary>
    public Guid ProductId { get; }

    /// <summary>
    /// Отображаемое название продукта.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Нормализованное название продукта.
    /// </summary>
    public string NormalizedName { get; }

    /// <summary>
    /// Количество.
    /// </summary>
    public decimal? Amount { get; }

    /// <summary>
    /// Единица измерения.
    /// </summary>
    public ShoppingUnit Unit { get; }

    /// <summary>
    /// Категория продукта.
    /// </summary>
    public ShoppingProductCategory Category { get; }

    /// <summary>
    /// Комментарий.
    /// </summary>
    public string? Comment { get; }

    /// <summary>
    /// Признак купленного продукта.
    /// </summary>
    public bool IsPurchased { get; init; }

    /// <summary>
    /// Возвращает true, если позицию можно безопасно объединять с похожими позициями.
    /// </summary>
    public bool CanMerge =>
        Amount.HasValue &&
        Unit is ShoppingUnit.Gram or ShoppingUnit.Milliliter or ShoppingUnit.Piece;

    /// <summary>
    /// Возвращает позицию, отмеченную как купленная.
    /// </summary>
    public ShoppingListItem MarkPurchased() => this with { IsPurchased = true };

}
