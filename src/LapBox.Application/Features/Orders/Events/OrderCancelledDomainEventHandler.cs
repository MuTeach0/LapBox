using LapBox.Application.Common.Events;
using LapBox.Application.Common.Interfaces.Notification;
using LapBox.Application.Common.Interfaces.Persistence;
using LapBox.Domain.Orders.Events;

namespace LapBox.Application.Features.Orders.Events;

public sealed class OrderCancelledDomainEventHandler(
    ICustomerRepository customerRepository,
    IUnitOfWork unitOfWork,
    IOrderNotifier orderNotifier) : IEventHandler<OrderCancelledEvent>
{
    public async Task HandleAsync(OrderCancelledEvent domainEvent, CancellationToken ct)
    {
        var customer = await customerRepository.GetByIdAsync(domainEvent.UserId, ct);
        if (customer is not null)
        {
            customer.RemoveCancelledPurchase(); 
            await unitOfWork.SaveChangesAsync(ct);
        }

        await orderNotifier.NotifyOrderCancelledAsync(domainEvent.OrderId, ct);
    }
}
