namespace MenuMate.SharedKernel;

/// <summary>
/// Базовый тип доменной сущности с типизированным идентификатором.
/// </summary>
/// <typeparam name="TId">Тип идентификатора сущности.</typeparam>
public abstract class Entity<TId>
    where TId : notnull
{
    private readonly List<IDomainEvent> _domainEvents = [];

    /// <summary>
    /// Инициализирует новый экземпляр <see cref="Entity{TId}" />.
    /// </summary>
    protected Entity(TId id)
    {
        Id = id;
    }

    /// <summary>
    /// Идентификатор сущности.
    /// </summary>
    public TId Id { get; }

    /// <summary>
    /// Снимок накопленных доменных событий.
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Очищает доменные события после публикации.
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    /// <summary>
    /// Добавляет доменное событие к сущности.
    /// </summary>
    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }
}
