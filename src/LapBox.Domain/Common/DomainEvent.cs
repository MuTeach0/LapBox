namespace LapBox.Domain.Common;

/// <summary>
/// Base class for all domain events.
/// Domain Layer must remain clean without external dependencies.
/// This record contains the event's metadata.
/// </summary>
public abstract record DomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}
