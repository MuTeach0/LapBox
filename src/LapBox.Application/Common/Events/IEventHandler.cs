using LapBox.Domain.Common;

namespace LapBox.Application.Common.Events;

/// <summary>
/// Interface for handling domain events.
/// Clean alternative to MediatR's INotificationHandler.
/// </summary>
/// <typeparam name="TEvent">The domain event type to handle</typeparam>
public interface IEventHandler<in TEvent> where TEvent : DomainEvent
{
    Task HandleAsync(TEvent domainEvent, CancellationToken cancellationToken = default);
}
