using LapBox.Application.Common.Interfaces.Events;
using LapBox.Domain.Common;
using Microsoft.Extensions.DependencyInjection;

namespace LapBox.Application.Common.Events;

/// <summary>
/// Default implementation of IDomainEventDispatcher that uses DI to find and invoke handlers.
/// </summary>
public sealed class DomainEventDispatcher(IServiceProvider serviceProvider) : IDomainEventDispatcher
{
    public async Task DispatchAsync(IEnumerable<DomainEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        foreach (var domainEvent in domainEvents)
        {
            var eventType = domainEvent.GetType();

            // Find handler for this event type
            var handlerType = typeof(IEventHandler<>).MakeGenericType(eventType);
            var handlers = serviceProvider.GetServices(handlerType);
            var handleMethod = handlerType.GetMethod(nameof(IEventHandler<DomainEvent>.HandleAsync));
            if (handleMethod is null) continue;

            foreach (var handler in handlers)
            {
                var task = (Task?)handleMethod.Invoke(handler, [domainEvent, cancellationToken]);
                if (task is not null)
                {
                    await task;
                }
            }
        }
    }
}
