using LapBox.Domain.Common;

namespace LapBox.Application.Common.Interfaces.Events;

/// <summary>
/// Dispatches domain events to their registered handlers.
/// </summary>
public interface IDomainEventDispatcher
{
    Task DispatchAsync(IEnumerable<DomainEvent> domainEvents, CancellationToken cancellationToken = default);
}
