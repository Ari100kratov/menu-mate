using MenuMate.SharedKernel;
using MenuMate.SharedKernel.Identifiers;

namespace MenuMate.Modules.ShoppingLists.Domain.Models;

/// <summary>
/// Сохраненный пользователем список покупок.
/// </summary>
public sealed class SavedShoppingList : Entity<Guid>
{
    private readonly List<SavedShoppingListItem> _items = [];

    private SavedShoppingList(
        Guid id,
        UserId ownerUserId,
        MenuPlanId sourceMenuPlanId,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt)
        : base(id)
    {
        OwnerUserId = ownerUserId;
        SourceMenuPlanId = sourceMenuPlanId;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    /// <summary>
    /// Пользователь, которому принадлежит список.
    /// </summary>
    public UserId OwnerUserId { get; }

    /// <summary>
    /// План меню, по которому был создан список.
    /// </summary>
    public MenuPlanId SourceMenuPlanId { get; }

    /// <summary>
    /// Момент создания списка.
    /// </summary>
    public DateTimeOffset CreatedAt { get; }

    /// <summary>
    /// Момент последнего изменения списка.
    /// </summary>
    public DateTimeOffset UpdatedAt { get; private set; }

    /// <summary>
    /// Позиции списка покупок.
    /// </summary>
    public IReadOnlyCollection<SavedShoppingListItem> Items => _items.AsReadOnly();

    /// <summary>
    /// Создает сохраненный список покупок из рассчитанных позиций.
    /// </summary>
    public static Result<SavedShoppingList> Create(
        Guid id,
        UserId ownerUserId,
        MenuPlanId sourceMenuPlanId,
        IEnumerable<ShoppingListItem> generatedItems,
        DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(generatedItems);

        var list = new SavedShoppingList(id, ownerUserId, sourceMenuPlanId, now, now);

        foreach (ShoppingListItem item in generatedItems)
        {
            Result<SavedShoppingListItem> savedItem = SavedShoppingListItem.FromGenerated(Guid.CreateVersion7(), item);
            if (savedItem.IsFailure)
            {
                return Result.Failure<SavedShoppingList>(savedItem.Error);
            }

            list._items.Add(savedItem.Value);
        }

        return list;
    }

    /// <summary>
    /// Восстанавливает список покупок из persistence-снимка.
    /// </summary>
    public static SavedShoppingList Rehydrate(
        Guid id,
        UserId ownerUserId,
        MenuPlanId sourceMenuPlanId,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt,
        IEnumerable<SavedShoppingListItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        var list = new SavedShoppingList(id, ownerUserId, sourceMenuPlanId, createdAt, updatedAt);
        list._items.AddRange(items);
        return list;
    }

    /// <summary>
    /// Добавляет ручную позицию.
    /// </summary>
    public void AddItem(SavedShoppingListItem item, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(item);

        _items.Add(item);
        UpdatedAt = now;
    }

    /// <summary>
    /// Обновляет позицию списка.
    /// </summary>
    public bool UpdateItem(Guid itemId, SavedShoppingListItem item, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(item);

        int index = _items.FindIndex(existing => existing.Id == itemId);
        if (index < 0)
        {
            return false;
        }

        _items[index] = item;
        UpdatedAt = now;
        return true;
    }

    /// <summary>
    /// Удаляет позицию списка.
    /// </summary>
    public bool RemoveItem(Guid itemId, DateTimeOffset now)
    {
        int removed = _items.RemoveAll(item => item.Id == itemId);
        if (removed == 0)
        {
            return false;
        }

        UpdatedAt = now;
        return true;
    }

    /// <summary>
    /// Обновляет отметки позиции.
    /// </summary>
    public bool SetItemState(Guid itemId, bool isPurchased, bool isInStock, DateTimeOffset now)
    {
        int index = _items.FindIndex(item => item.Id == itemId);
        if (index < 0)
        {
            return false;
        }

        _items[index] = _items[index].WithState(isPurchased, isInStock);
        UpdatedAt = now;
        return true;
    }

    /// <summary>
    /// Возвращает сгруппированное представление списка.
    /// </summary>
    public ShoppingList ToGroupedShoppingList() =>
        ShoppingList.FromItems(_items.Select(item => item.ToShoppingListItem()));
}
