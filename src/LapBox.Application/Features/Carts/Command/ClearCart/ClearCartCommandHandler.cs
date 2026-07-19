using LapBox.Application.Common.Interfaces.Persistence;
using LapBox.Domain.Carts;
using LapBox.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace LapBox.Application.Features.Carts.Command.ClearCart;

public sealed class ClearCartCommandHandler(
    ICartRepository cartRepository,
    IStockReservationRepository reservationRepository,
    ILaptopRepository laptopRepository,
    IUnitOfWork unitOfWork,
    HybridCache cache,
    ILogger<ClearCartCommandHandler> logger) : IRequestHandler<ClearCartCommand, Result<Success>>
{
    public async Task<Result<Success>> Handle(ClearCartCommand command, CancellationToken ct)
    {
        logger.LogInformation("Attempting to clear cart for identity: {IdentityId}", command.IdentityId);

        var cart = await cartRepository.GetByIdentityIdAsync(command.IdentityId, ct);
        
        if (cart is null)
        {
            logger.LogWarning("Cart not found for identity {IdentityId} to clear.", command.IdentityId);
            return CartErrors.ItemNotFound;
        }

        // استدعاء الـ Domain Logic
        var activeReservations = await reservationRepository.GetActiveByUserIdAsync(command.IdentityId, ct);
        foreach (var reservation in activeReservations)
        {
            if (reservation.Release().IsError) continue;

            var laptop = await laptopRepository.GetByIdAsync(reservation.LaptopId, ct);
            laptop?.UpdateInventory(reservation.Quantity);
        }

        cart.Clear(command.CheckedOut);

        // حفظ التغييرات
        await unitOfWork.SaveChangesAsync(ct);

        // تطهير الكاش المخصص لهذا المستخدم
        await cache.RemoveAsync($"cart_{command.IdentityId}", ct);

        logger.LogInformation("Cart cleared and cache evicted successfully for identity: {IdentityId}", command.IdentityId);

        return Result.Success;
    }
}
