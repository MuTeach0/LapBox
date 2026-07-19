using LapBox.Application.Common.Interfaces.Persistence;
using LapBox.Domain.Carts;
using LapBox.Domain.Common.Results;
using LapBox.Domain.StockReservations;
using MediatR;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace LapBox.Application.Features.Carts.Command.AddOrUpdateCartItem;

public sealed class AddOrUpdateCartItemCommandHandler(
    ICartRepository cartRepository,
    ILaptopRepository laptopRepository,
    IStockReservationRepository reservationRepository,
    IUnitOfWork unitOfWork,
    HybridCache cache,
    ILogger<AddOrUpdateCartItemCommandHandler> logger) : IRequestHandler<AddOrUpdateCartItemCommand, Result<Success>>
{
    public async Task<Result<Success>> Handle(AddOrUpdateCartItemCommand command, CancellationToken ct)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync(ct);
        try
        {
            var laptop = (await laptopRepository.GetByIdsWithLockAsync([command.LaptopId], ct)).SingleOrDefault();
            if (laptop is null)
                return Error.NotFound("Laptop.NotFound", "Laptop was not found.");

            var inventoryResult = laptop.UpdateInventory(-command.Quantity);
            if (inventoryResult.IsError)
                return inventoryResult.Errors;

            var reservations = await reservationRepository.GetActiveByUserAndLaptopsAsync(
                command.IdentityId, [command.LaptopId], ct);
            var reservation = reservations.FirstOrDefault(r => r.ExpiresAtUtc > DateTimeOffset.UtcNow);
            if (reservation is null)
            {
                var reservationResult = StockReservation.CreateTemporary(command.LaptopId, command.IdentityId, command.Quantity);
                if (reservationResult.IsError) return reservationResult.Errors;
                await reservationRepository.AddAsync(reservationResult.Value, ct);
            }
            else
            {
                var reservationResult = reservation.AddQuantity(command.Quantity);
                if (reservationResult.IsError) return reservationResult.Errors;
            }

            var cart = await cartRepository.GetByIdentityIdAsync(command.IdentityId, ct);
            if (cart is null)
            {
                var createResult = Cart.Create(command.IdentityId);
                if (createResult.IsError) return createResult.Errors;
                cart = createResult.Value;
                await cartRepository.AddAsync(cart, ct);
            }

            var addResult = cart.AddOrUpdateItem(command.LaptopId, command.Quantity, command.UnitPrice);
            if (addResult.IsError) return addResult.Errors;
           
            cart.UpdateLastModified();

            // _dbContext.Entry(cart).State = EntityState.Modified;
            cartRepository.Update(cart, ct);

            await unitOfWork.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
            await cache.RemoveAsync($"cart_{command.IdentityId}", ct);
            logger.LogInformation("Reserved {Quantity} unit(s) of {LaptopId} for identity {IdentityId}", command.Quantity, command.LaptopId, command.IdentityId);
            return Result.Success;
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }
}
