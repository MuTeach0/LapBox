using LapBox.Application.Common.Interfaces.Persistence;
using LapBox.Domain.Carts;
using LapBox.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace LapBox.Application.Features.Carts.Command.RemoveCartItem;

public sealed class RemoveCartItemCommandHandler(
    ICartRepository cartRepository,
    IStockReservationRepository reservationRepository,
    ILaptopRepository laptopRepository,
    IUnitOfWork unitOfWork,
    HybridCache cache,
    ILogger<RemoveCartItemCommandHandler> logger) : IRequestHandler<RemoveCartItemCommand, Result<Success>>
{
    public async Task<Result<Success>> Handle(RemoveCartItemCommand command, CancellationToken ct)
    {
        logger.LogInformation("Removing laptop {LaptopId} from cart for identity {IdentityId}", command.LaptopId, command.IdentityId);

        var cart = await cartRepository.GetByIdentityIdAsync(command.IdentityId, ct);
        if (cart is null)
        {
            return CartErrors.ItemNotFound;
        }

        var result = cart.RemoveItem(command.LaptopId);
        if (result.IsError)
        {
            return result.Errors;
        }

        var reservations = await reservationRepository.GetActiveByUserAndLaptopsAsync(
            command.IdentityId, [command.LaptopId], ct);
        var releasedQuantity = 0;
        foreach (var reservation in reservations)
        {
            if (!reservation.Release().IsError)
                releasedQuantity += reservation.Quantity;
        }

        if (releasedQuantity > 0)
        {
            var laptop = await laptopRepository.GetByIdAsync(command.LaptopId, ct);
            laptop?.UpdateInventory(releasedQuantity);
        }

        await unitOfWork.SaveChangesAsync(ct);

        // تطيير الكاش الخاص بالمستخدم ده
        await cache.RemoveAsync($"cart_{command.IdentityId}", ct);

        logger.LogInformation("Item removed and cache cleared for identity: {IdentityId}", command.IdentityId);
        return Result.Success;
    }
}
