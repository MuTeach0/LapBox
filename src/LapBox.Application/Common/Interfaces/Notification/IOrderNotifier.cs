namespace LapBox.Application.Common.Interfaces.Notification;

public interface IOrderNotifier
{
    Task NotifyOrderPlacedAsync(Guid orderId, CancellationToken ct = default); // 🚀 الميثود الجديدة
    Task NotifyOrderDispatchedAsync(Guid orderId, string trackingLabel, CancellationToken ct = default);
    Task NotifyOrderDeliveredAsync(Guid orderId, CancellationToken ct = default);
    Task NotifyOrderCancelledAsync(Guid orderId, CancellationToken ct = default);
}