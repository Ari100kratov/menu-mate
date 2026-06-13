using MenuMate.Modules.ShoppingLists.Domain.Enums;
using MenuMate.Modules.ShoppingLists.Domain.Errors;
using MenuMate.SharedKernel;

namespace MenuMate.Modules.ShoppingLists.Domain.Models;

/// <summary>
/// Сохраненная позиция списка покупок.
/// </summary>
public sealed record SavedShoppingListItem
{
    private SavedShoppingListItem(
        Guid id,
        Guid productId,
        string name,
        string normalizedName,
        decimal? amount,
        ShoppingUnit unit,
        ShoppingProductCategory category,
        string? comment,
        bool isPurchased)
    {
        Id = id;
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
    /// Идентификатор позиции.
    /// </summary>
    public Guid Id { get; }

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
    /// Числовое количество.
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
    /// Комментарий к позиции.
    /// </summary>
    public string? Comment { get; }

    /// <summary>
    /// Признак купленного продукта.
    /// </summary>
    public bool IsPurchased { get; init; }

    /// <summary>
    /// Создает сохраненную позицию списка покупок.
    /// </summary>
    public static Result<SavedShoppingListItem> Create(
        Guid id,
        Guid productId,
        string name,
        string normalizedName,
        decimal? amount,
        ShoppingUnit unit,
        ShoppingProductCategory category,
        string? comment,
        bool isPurchased = false)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<SavedShoppingListItem>(ShoppingListErrors.EmptyItemName);
        }

        if (unit != ShoppingUnit.ToTaste && amount is null)
        {
            return Result.Failure<SavedShoppingListItem>(ShoppingListErrors.AmountRequired);
        }

        if (amount <= 0)
        {
            return Result.Failure<SavedShoppingListItem>(ShoppingListErrors.AmountMustBePositive);
        }

        string trimmedName = name.Trim();
        string normalized = string.IsNullOrWhiteSpace(normalizedName)
            ? TextNormalizer.NormalizeSearchText(trimmedName)
            : normalizedName.Trim();

        decimal? effectiveAmount = unit == ShoppingUnit.ToTaste ? null : amount;

        return new SavedShoppingListItem(
            id,
            productId,
            trimmedName,
            normalized,
            effectiveAmount,
            unit,
            category,
            string.IsNullOrWhiteSpace(comment) ? null : comment.Trim(),
            isPurchased);
    }

    /// <summary>
    /// Создает сохраненную позицию из рассчитанной позиции списка покупок.
    /// </summary>
    public static Result<SavedShoppingListItem> FromGenerated(Guid id, ShoppingListItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        return Create(
            id,
            item.ProductId,
            item.Name,
            item.NormalizedName,
            item.Amount,
            item.Unit,
            item.Category,
            item.Comment,
            item.IsPurchased);
    }

    /// <summary>
    /// Возвращает позицию для группировки и текстового представления.
    /// </summary>
    public ShoppingListItem ToShoppingListItem() =>
        new(
            ProductId,
            Name,
            NormalizedName,
            Amount,
            Unit,
            Category,
            Comment,
            IsPurchased);

    /// <summary>
    /// Возвращает позицию с обновленными чекбоксами.
    /// </summary>
    public SavedShoppingListItem WithState(bool isPurchased) =>
        this with { IsPurchased = isPurchased };
}
