using LapBox.Application.Common.Interfaces.Persistence;
using LapBox.Application.Common.Interfaces.Policies;
using LapBox.Domain.StockReservations.Enums;

namespace LapBox.Infrastructure.Services;

public sealed class LaptopPolicy(
    ILaptopRepository laptopRepository,
    IStockReservationRepository reservationRepository) : ILaptopPolicy
{
    /// <summary>
    /// Checks if the requested quantity is available considering:
    /// 1. Laptop must exist and be active
    /// 2. Physical stock minus active reservations must cover the request
    /// </summary>
    public async Task<bool> IsStockAvailableAsync(
        Guid laptopId,
        int quantity,
        CancellationToken ct = default)
    {
        var laptop = await laptopRepository.GetByIdAsync(laptopId, ct);
        if (laptop is null || !laptop.IsActive) return false;

        var reservedQty = await reservationRepository.GetActiveReservedQuantityAsync(laptopId, ct);
        var availableStock = laptop.InventoryQuantity - reservedQty;

        return availableStock >= quantity;
    }
}
