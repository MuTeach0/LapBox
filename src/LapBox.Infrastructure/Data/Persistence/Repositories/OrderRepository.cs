using LapBox.Application.Common.Interfaces.Persistence;
using LapBox.Domain.Orders;
using LapBox.Domain.Orders.Enums;
using Microsoft.EntityFrameworkCore;

namespace LapBox.Infrastructure.Data.Persistence.Repositories;

public sealed class OrderRepository(AppDbContext dbContext)
    : Repository<Order>(dbContext), IOrderRepository
{
    public async Task<Order?> GetOrderWithItemsAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);
    }

    public async Task<List<Order>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await dbContext.Orders
            .Include(o => o.OrderItems)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync(ct);
    }

    public async Task<Order?> GetByTrackingLabelAsync(string trackingLabel, CancellationToken ct = default)
    {
        return await dbContext.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.TrackingLabel == trackingLabel, ct);
    }

    public async Task<List<Order>> GetByStatusAsync(OrderStatus status, CancellationToken ct = default)
    {
        return await dbContext.Orders
            .Where(o => o.Status == status)
            .ToListAsync(ct);
    }

    // الـ Pagination الذكي لحساب الصفحات وعرض الداتا بكفاءة
    public async Task<(List<Order> Items, int TotalCount)> GetByUserIdPaginatedAsync(
        Guid userId, int page, int pageSize, CancellationToken ct)
    {
        var query = dbContext.Orders
            .Include(o => o.OrderItems)
            .Where(o => o.UserId == userId);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(o => o.OrderDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task<(List<Order> Items, int TotalCount)> GetAllPaginatedAsync(int page, int pageSize, CancellationToken ct = default)
    {
        var query = dbContext.Orders.Include(o => o.OrderItems); // Include items if needed

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(o => o.OrderDate) // الأحدث أولاً
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }
}