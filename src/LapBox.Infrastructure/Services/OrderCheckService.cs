using LapBox.Application.Common.Interfaces;
using LapBox.Application.Common.Interfaces.Persistence;
using LapBox.Domain.Orders.Enums;
using LapBox.Infrastructure.Data.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LapBox.Infrastructure.Services;

public sealed class OrderCheckService(AppDbContext dbContext) : IOrderCheckService
{
    public async Task<bool> HasUserPurchasedLaptopAsync(
        Guid userId,
        Guid laptopId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Orders
            .Where(o => o.UserId == userId && o.Status == OrderStatus.Delivered)
            .SelectMany(o => o.OrderItems)
            .AnyAsync(oi => oi.LaptopId == laptopId, cancellationToken);
    }
}
