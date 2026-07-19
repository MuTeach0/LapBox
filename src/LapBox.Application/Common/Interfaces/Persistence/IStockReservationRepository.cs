using LapBox.Domain.StockReservations;

namespace LapBox.Application.Common.Interfaces.Persistence;

public interface IStockReservationRepository: IRepository<StockReservation>
{
    /// <summary>
    /// Returns all active reservations that have expired and need cleanup.
    /// </summary>
    Task<List<StockReservation>> GetExpiredActiveReservationsAsync(CancellationToken ct = default);

    /// <summary>
    /// Returns the total quantity of active (non-expired) reservations for a laptop.
    /// </summary>
    Task<int> GetActiveReservedQuantityAsync(Guid laptopId, CancellationToken ct = default);

    /// <summary>
    /// Returns all active reservations for a specific user (by IdentityId).
    /// Used when creating an Order from temporary reservations.
    /// </summary>
    Task<List<StockReservation>> GetActiveByUserIdAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Returns active reservations for a specific user and list of laptop IDs.
    /// Used to validate reservations match order items.
    /// </summary>
    Task<List<StockReservation>> GetActiveByUserAndLaptopsAsync(
        Guid userId, 
        List<Guid> laptopIds, 
        CancellationToken ct = default);

    /// <summary>
    /// Checks if a user has any active reservations.
    /// </summary>
    Task<bool> HasActiveReservationsAsync(Guid userId, CancellationToken ct = default);
}