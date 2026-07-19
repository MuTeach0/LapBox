using LapBox.Application.Common.Events;
using LapBox.Application.Common.Interfaces.Notification;
using LapBox.Domain.Customers.Events;

namespace LapBox.Application.Features.Customers.Events;

public sealed class CustomerCreatedDomainEventHandler(INotificationService notificationService)
    : IEventHandler<CustomerCreatedEvent>
{
    public async Task HandleAsync(CustomerCreatedEvent domainEvent, CancellationToken ct)
    {
        string message = "Welcome to LapBox!";
        await notificationService.SendEmailAsync(domainEvent.Email, message, ct);
    }
}
