namespace MenuMate.SharedKernel.UnitTests.Entities;

public sealed class EntityTests
{
    [Fact]
    public void DomainEventsShouldBeAddedAndCleared()
    {
        var entity = new TestEntity(Guid.CreateVersion7());
        var domainEvent = new TestDomainEvent(DateTimeOffset.UtcNow);

        entity.Raise(domainEvent);
        Assert.Same(domainEvent, Assert.Single(entity.DomainEvents));

        entity.ClearDomainEvents();
        Assert.Empty(entity.DomainEvents);
    }

    private sealed class TestEntity(Guid id) : Entity<Guid>(id)
    {
        public void Raise(IDomainEvent domainEvent) => AddDomainEvent(domainEvent);
    }

    private sealed record TestDomainEvent(DateTimeOffset OccurredAt) : IDomainEvent;
}
