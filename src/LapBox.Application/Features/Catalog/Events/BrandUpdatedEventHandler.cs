using LapBox.Application.Common.Events;
using LapBox.Domain.Catalog.Events.BrandEvents;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace LapBox.Application.Features.Catalog.Events;

public class BrandUpdatedEventHandler(
    HybridCache cache,
    ILogger<BrandUpdatedEventHandler> logger) : IEventHandler<BrandUpdatedEvent>
{
    public async Task HandleAsync(BrandUpdatedEvent domainEvent, CancellationToken ct)
    {
        logger.LogInformation("Handling BrandUpdatedEvent for Brand ID: {BrandId}. Evicting related caches...", domainEvent.BrandId);

        await cache.RemoveByTagAsync("brands", ct);
        await cache.RemoveByTagAsync("laptops", ct);
    }
}
