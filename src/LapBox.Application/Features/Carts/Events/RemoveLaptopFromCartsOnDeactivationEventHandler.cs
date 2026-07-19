using LapBox.Application.Common.Events;
using LapBox.Application.Common.Interfaces.Persistence;
using LapBox.Domain.Laptops.Events;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace LapBox.Application.Features.Carts.Events;

public sealed class RemoveLaptopFromCartsOnDeactivationEventHandler(
    ICartRepository cartRepository,
    IUnitOfWork unitOfWork,
    HybridCache cache,
    ILogger<RemoveLaptopFromCartsOnDeactivationEventHandler> logger) 
    : IEventHandler<LaptopDeactivatedEvent>
{
    public async Task HandleAsync(LaptopDeactivatedEvent domainEvent, CancellationToken ct)
    {
        logger.LogInformation("Processing LaptopDeactivatedEvent. Removing LaptopId: {LaptopId} from all active carts.", 
            domainEvent.LaptopId);

        var affectedIdentityIds = await cartRepository.RemoveProductFromAllCartsAsync(domainEvent.LaptopId, ct);
        
        if (affectedIdentityIds.Count == 0)
        {
            logger.LogInformation("No active carts contained the deactivated LaptopId: {LaptopId}.", domainEvent.LaptopId);
            return;
        }

        await unitOfWork.SaveChangesAsync(ct);

        foreach (var identityId in affectedIdentityIds)
        {
            await cache.RemoveAsync($"cart_{identityId}", ct);
        }

        logger.LogInformation("Successfully removed deactivated LaptopId: {LaptopId} from {Count} carts and evicted their cache.", 
            domainEvent.LaptopId, affectedIdentityIds.Count);
    }
}
