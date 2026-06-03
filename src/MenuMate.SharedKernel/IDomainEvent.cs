namespace MenuMate.SharedKernel;

/// <summary>
/// Маркер доменного события.
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// Момент возникновения события в UTC.
    /// </summary>
    DateTimeOffset OccurredAt { get; }
}

