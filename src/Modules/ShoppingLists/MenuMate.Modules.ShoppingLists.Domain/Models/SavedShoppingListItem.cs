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
        string name,
        string normalizedName,
        decimal? amount,
        ShoppingUnit unit,
        ShoppingQuantityKind quantityKind,
        ShoppingProductCategory category,
        string? comment,
        bool isPurchased,
        bool isInStock)
    {
        Id = id;
        Name = name;
        NormalizedName = normalizedName;
        Amount = amount;
        Unit = unit;
        QuantityKind = quantityKind;
        Category = category;
        Comment = comment;
        IsPurchased = isPurchased;
        IsInStock = isInStock;
    }

    /// <summary>
    /// Идентификатор позиции.
    /// </summary>
    public Guid Id { get; }

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
    /// Тип количества.
    /// </summary>
    public ShoppingQuantityKind QuantityKind { get; }

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
    /// Признак продукта, который уже есть дома.
    /// </summary>
    public bool IsInStock { get; init; }

    /// <summary>
    /// Создает сохраненную позицию списка покупок.
    /// </summary>
    public static Result<SavedShoppingListItem> Create(
        Guid id,
        string name,
        string normalizedName,
        decimal? amount,
        ShoppingUnit unit,
        ShoppingQuantityKind quantityKind,
        ShoppingProductCategory category,
        string? comment,
        bool isPurchased = false,
        bool isInStock = false)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<SavedShoppingListItem>(ShoppingListErrors.EmptyItemName);
        }

        if (quantityKind != ShoppingQuantityKind.ToTaste && amount is null)
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

        decimal? effectiveAmount = quantityKind == ShoppingQuantityKind.ToTaste ? null : amount;
        ShoppingUnit effectiveUnit = quantityKind == ShoppingQuantityKind.ToTaste ? ShoppingUnit.ToTaste : unit;

        return new SavedShoppingListItem(
            id,
            trimmedName,
            normalized,
            effectiveAmount,
            effectiveUnit,
            quantityKind,
            category,
            string.IsNullOrWhiteSpace(comment) ? null : comment.Trim(),
            isPurchased,
            isInStock);
    }

    /// <summary>
    /// Создает сохраненную позицию из рассчитанной позиции списка покупок.
    /// </summary>
    public static Result<SavedShoppingListItem> FromGenerated(Guid id, ShoppingListItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        return Create(
            id,
            item.Name,
            item.NormalizedName,
            item.Amount,
            item.Unit,
            item.QuantityKind,
            item.Category,
            item.Comment,
            item.IsPurchased,
            item.IsInStock);
    }

    /// <summary>
    /// Возвращает позицию для группировки и текстового представления.
    /// </summary>
    public ShoppingListItem ToShoppingListItem() =>
        new(
            Name,
            NormalizedName,
            Amount,
            Unit,
            QuantityKind,
            Category,
            Comment,
            IsPurchased,
            IsInStock);

    /// <summary>
    /// Возвращает позицию с обновленными чекбоксами.
    /// </summary>
    public SavedShoppingListItem WithState(bool isPurchased, bool isInStock) =>
        this with { IsPurchased = isPurchased, IsInStock = isInStock };
}
