using LapBox.Application.Common.Interfaces.Persistence;
using LapBox.Domain.StockReservations;
using LapBox.Domain.StockReservations.Enums;
using Microsoft.EntityFrameworkCore;

namespace LapBox.Infrastructure.Data.Persistence.Repositories;

public sealed class StockReservationRepository(AppDbContext dbContext)
    : Repository<StockReservation>(dbContext), IStockReservationRepository
{
    /// <summary>
    /// Returns all active (not yet consumed/released) reservations that have expired.
    /// Called by the background cleanup job.
    /// </summary>
    public async Task<List<StockReservation>> GetExpiredActiveReservationsAsync(CancellationToken ct = default)
    {
        var now = DateTimeOffset.UtcNow;

        return await dbContext.StockReservations
            .Where(r => r.Status == ReservationStatus.Active && r.ExpiresAtUtc < now)
            .ToListAsync(ct);
    }

    /// <summary>
    /// Returns active reservations for a specific laptop to check stock availability.
    /// </summary>
    public async Task<int> GetActiveReservedQuantityAsync(Guid laptopId, CancellationToken ct = default)
    {
        return await dbContext.StockReservations
            .Where(r => r.LaptopId == laptopId && r.Status == ReservationStatus.Active)
            .SumAsync(r => r.Quantity, ct);
    }

    /// <summary>
    /// Returns all active reservations for a specific user (by IdentityId).
    /// Used when creating an Order from temporary reservations.
    /// </summary>
    public async Task<List<StockReservation>> GetActiveByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await dbContext.StockReservations
            .Where(r => r.UserId == userId && r.Status == ReservationStatus.Active)
            .ToListAsync(ct);
    }

    /// <summary>
    /// Returns active reservations for a specific user and list of laptop IDs.
    /// Used to validate reservations match order items.
    /// </summary>
    public async Task<List<StockReservation>> GetActiveByUserAndLaptopsAsync(
        Guid userId, 
        List<Guid> laptopIds, 
        CancellationToken ct = default)
    {
        return await dbContext.StockReservations
            .Where(r => r.UserId == userId 
                     && r.Status == ReservationStatus.Active
                     && laptopIds.Contains(r.LaptopId))
            .ToListAsync(ct);
    }

    /// <summary>
    /// Checks if a user has any active reservations.
    /// </summary>
    public async Task<bool> HasActiveReservationsAsync(Guid userId, CancellationToken ct = default)
    {
        return await dbContext.StockReservations
            .AnyAsync(r => r.UserId == userId && r.Status == ReservationStatus.Active, ct);
    }
}
