using LapBox.Application.Common.Events;
using LapBox.Application.Common.Interfaces.Notification;
using LapBox.Domain.Orders.Enums;
using LapBox.Domain.Orders.Events;
using Microsoft.Extensions.Logging;

namespace LapBox.Application.Features.Orders.Events;

public sealed class OrderStatusChangedDomainEventHandler(
    ILogger<OrderStatusChangedDomainEventHandler> logger,
    IOrderNotifier orderNotifier) : IEventHandler<OrderStatusChangedDomainEvent>
{
    public async Task HandleAsync(OrderStatusChangedDomainEvent domainEvent, CancellationToken ct)
    {
        logger.LogInformation("Order {OrderId} status changed to {NewStatus}",
            domainEvent.OrderId, domainEvent.NewStatus);

        // بناءً على الحالة الجديدة، نبعت الإشعار المناسب للعميل
        switch (domainEvent.NewStatus)
        {
            case OrderStatus.Dispatched:
                await orderNotifier.NotifyOrderDispatchedAsync(domainEvent.OrderId, domainEvent.TrackingLabel!, ct);
                break;

            case OrderStatus.Delivered:
                await orderNotifier.NotifyOrderDeliveredAsync(domainEvent.OrderId, ct);
                break;

            // ممكن تضيف حالات تانية مستقبلاً
        }
    }
}