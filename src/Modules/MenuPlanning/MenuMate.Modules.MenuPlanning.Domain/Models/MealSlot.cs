using MenuMate.Modules.MenuPlanning.Domain.Errors;
using MenuMate.SharedKernel;
using MenuMate.SharedKernel.Identifiers;

namespace MenuMate.Modules.MenuPlanning.Domain.Models;

/// <summary>
/// Настраиваемый прием пищи пользователя.
/// </summary>
public sealed class MealSlot : Entity<Guid>
{
    /// <summary>
    /// Максимальная длина названия приема пищи.
    /// </summary>
    public const int MaxNameLength = 60;

    private MealSlot(
        Guid id,
        UserId ownerUserId,
        string name,
        int sortOrder,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt)
        : base(id)
    {
        OwnerUserId = ownerUserId;
        Name = name;
        SortOrder = sortOrder;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }

    /// <summary>
    /// Пользователь, которому принадлежит прием пищи.
    /// </summary>
    public UserId OwnerUserId { get; }

    /// <summary>
    /// Название приема пищи.
    /// </summary>
    public string Name { get; private set; }

    /// <summary>
    /// Порядок отображения.
    /// </summary>
    public int SortOrder { get; private set; }

    /// <summary>
    /// Момент создания.
    /// </summary>
    public DateTimeOffset CreatedAt { get; }

    /// <summary>
    /// Момент последнего изменения.
    /// </summary>
    public DateTimeOffset UpdatedAt { get; private set; }

    /// <summary>
    /// Создает прием пищи.
    /// </summary>
    public static Result<MealSlot> Create(
        Guid id,
        UserId ownerUserId,
        string name,
        int sortOrder,
        DateTimeOffset now)
    {
        Result<string> normalizedName = NormalizeName(name);
        if (normalizedName.IsFailure)
        {
            return Result.Failure<MealSlot>(normalizedName.Error);
        }

        return new MealSlot(id, ownerUserId, normalizedName.Value, sortOrder, now, now);
    }

    /// <summary>
    /// Восстанавливает прием пищи из persistence-снимка.
    /// </summary>
    public static MealSlot Rehydrate(
        Guid id,
        UserId ownerUserId,
        string name,
        int sortOrder,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt) =>
        new(id, ownerUserId, name, sortOrder, createdAt, updatedAt);

    /// <summary>
    /// Переименовывает прием пищи.
    /// </summary>
    public Result Rename(string name, DateTimeOffset now)
    {
        Result<string> normalizedName = NormalizeName(name);
        if (normalizedName.IsFailure)
        {
            return Result.Failure(normalizedName.Error);
        }

        Name = normalizedName.Value;
        UpdatedAt = now;
        return Result.Success();
    }

    /// <summary>
    /// Меняет порядок отображения.
    /// </summary>
    public void ChangeSortOrder(int sortOrder, DateTimeOffset now)
    {
        SortOrder = sortOrder;
        UpdatedAt = now;
    }

    private static Result<string> NormalizeName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Failure<string>(MenuCalendarErrors.EmptyMealSlotName);
        }

        string normalized = value.Trim();
        return normalized.Length > MaxNameLength
            ? Result.Failure<string>(MenuCalendarErrors.MealSlotNameTooLong)
            : normalized;
    }
}
