using LapBox.Domain.Orders;
using LapBox.Domain.Orders.Enums;

namespace LapBox.Application.Common.Interfaces.Persistence;

public interface IOrderRepository : IRepository<Order>
{
    // هنا نضع العمليات الخاصة بالـ Order فقط
    Task<Order?> GetOrderWithItemsAsync(Guid orderId, CancellationToken cancellationToken = default);

    // استعلام طلبات عميل معين
    Task<List<Order>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);

    // للبحث برقم التتبع أو الحالة (أمثلة)
    Task<Order?> GetByTrackingLabelAsync(string trackingLabel, CancellationToken ct = default);

    Task<List<Order>> GetByStatusAsync(OrderStatus status, CancellationToken ct = default);
    Task<(List<Order> Items, int TotalCount)> GetByUserIdPaginatedAsync(Guid userId, int page, int pageSize, CancellationToken ct);
    Task<(List<Order> Items, int TotalCount)> GetAllPaginatedAsync(int page, int pageSize, CancellationToken ct = default);
}