using MenuMate.Modules.Tags.Domain.Enums;
using MenuMate.Modules.Tags.Domain.ValueObjects;
using MenuMate.SharedKernel;

namespace MenuMate.Modules.Tags.Domain.Models;

/// <summary>
/// Тег для поиска, фильтрации и будущей персонализации рецептов.
/// </summary>
public sealed class Tag : Entity<Guid>
{
    private Tag(Guid id, TagName name, TagKind kind, TagStatus status, DateTimeOffset createdAt)
        : base(id)
    {
        Name = name;
        Kind = kind;
        Status = status;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    /// <summary>
    /// Название тега.
    /// </summary>
    public TagName Name { get; private set; }

    /// <summary>
    /// Источник тега.
    /// </summary>
    public TagKind Kind { get; }

    /// <summary>
    /// Статус подтверждения тега.
    /// </summary>
    public TagStatus Status { get; private set; }

    /// <summary>
    /// Момент создания тега.
    /// </summary>
    public DateTimeOffset CreatedAt { get; }

    /// <summary>
    /// Момент последнего изменения тега.
    /// </summary>
    public DateTimeOffset UpdatedAt { get; private set; }

    /// <summary>
    /// Создает тег.
    /// </summary>
    public static Tag Create(Guid id, TagName name, TagKind kind, DateTimeOffset now)
    {
        TagStatus status = kind == TagKind.Suggested ? TagStatus.Proposed : TagStatus.Confirmed;
        return new Tag(id, name, kind, status, now);
    }

    /// <summary>
    /// Rehydrates a tag from persistence.
    /// </summary>
    public static Tag Rehydrate(
        Guid id,
        TagName name,
        TagKind kind,
        TagStatus status,
        DateTimeOffset createdAt,
        DateTimeOffset updatedAt) =>
        new(id, name, kind, status, createdAt)
        {
            UpdatedAt = updatedAt
        };

    /// <summary>
    /// Подтверждает предложенный тег.
    /// </summary>
    public void Confirm(DateTimeOffset now)
    {
        Status = TagStatus.Confirmed;
        UpdatedAt = now;
    }

    /// <summary>
    /// Скрывает тег без физического удаления.
    /// </summary>
    public void Hide(DateTimeOffset now)
    {
        Status = TagStatus.Hidden;
        UpdatedAt = now;
    }
}
