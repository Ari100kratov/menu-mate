using MenuMate.Modules.MenuPlanning.Domain.Errors;
using MenuMate.Modules.MenuPlanning.Domain.ValueObjects;
using MenuMate.SharedKernel;
using MenuMate.SharedKernel.Identifiers;

namespace MenuMate.Modules.MenuPlanning.Domain.Models;

/// <summary>
/// План меню на неделю, месяц или произвольный диапазон дат.
/// </summary>
public sealed class MenuPlan : Entity<Guid>
{
    private readonly List<MenuPlanItem> _items = [];

    private MenuPlan(Guid id, UserId ownerUserId, string name, MenuPlanDateRange dateRange, DateTimeOffset now)
        : base(id)
    {
        OwnerUserId = ownerUserId;
        Name = name;
        DateRange = dateRange;
        CreatedAt = now;
        UpdatedAt = now;
    }

    /// <summary>
    /// Пользователь, которому принадлежит план меню.
    /// </summary>
    public UserId OwnerUserId { get; }

    /// <summary>
    /// Название плана.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Диапазон дат плана.
    /// </summary>
    public MenuPlanDateRange DateRange { get; private set; }

    /// <summary>
    /// Момент создания.
    /// </summary>
    public DateTimeOffset CreatedAt { get; }

    /// <summary>
    /// Момент последнего изменения.
    /// </summary>
    public DateTimeOffset UpdatedAt { get; private set; }

    /// <summary>
    /// Позиции меню.
    /// </summary>
    public IReadOnlyCollection<MenuPlanItem> Items => _items.AsReadOnly();

    /// <summary>
    /// Создает план меню.
    /// </summary>
    public static Result<MenuPlan> Create(
        Guid id,
        UserId ownerUserId,
        string name,
        MenuPlanDateRange dateRange,
        DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<MenuPlan>(MenuPlanErrors.EmptyName);
        }

        return new MenuPlan(id, ownerUserId, name.Trim(), dateRange, now);
    }

    /// <summary>
    /// Восстанавливает план меню из persistence-снимка.
    /// </summary>
    public static MenuPlan Rehydrate(
        Guid id,
        UserId ownerUserId,
        string name,
        MenuPlanDateRange dateRange,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt,
        IEnumerable<MenuPlanItem> items)
    {
        var menuPlan = new MenuPlan(id, ownerUserId, name, dateRange, createdAt)
        {
            UpdatedAt = updatedAt
        };

        menuPlan._items.AddRange(items);

        return menuPlan;
    }

    /// <summary>
    /// Добавляет позицию в меню.
    /// </summary>
    public Result AddItem(MenuPlanItem item, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (!DateRange.Contains(item.Date))
        {
            return Result.Failure(MenuPlanErrors.DateOutsidePlanRange);
        }

        _items.Add(item);
        UpdatedAt = now;
        return Result.Success();
    }

    /// <summary>
    /// Обновляет позицию меню.
    /// </summary>
    public Result UpdateItem(MenuPlanItem item, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (!DateRange.Contains(item.Date))
        {
            return Result.Failure(MenuPlanErrors.DateOutsidePlanRange);
        }

        int index = _items.FindIndex(existing => existing.Id == item.Id);
        if (index < 0)
        {
            return Result.Failure(MenuPlanErrors.ItemNotFound);
        }

        _items[index] = item;
        UpdatedAt = now;
        return Result.Success();
    }

    /// <summary>
    /// Обновляет основные параметры плана меню.
    /// </summary>
    public Result UpdateDetails(string name, MenuPlanDateRange dateRange, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(dateRange);

        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure(MenuPlanErrors.EmptyName);
        }

        if (_items.Any(item => !dateRange.Contains(item.Date)))
        {
            return Result.Failure(MenuPlanErrors.ItemsOutsidePlanRange);
        }

        Name = name.Trim();
        DateRange = dateRange;
        UpdatedAt = now;
        return Result.Success();
    }

    /// <summary>
    /// Удаляет позицию меню.
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
}
