using LapBox.Application.Common.Events;
using LapBox.Application.Common.Interfaces.Notification;
using LapBox.Domain.Laptops.Events;
using Microsoft.Extensions.Logging;

namespace LapBox.Application.Features.Laptops.Events;

public class LaptopPriceChangedEventHandler(
    INotificationService notificationService,
    ILogger<LaptopPriceChangedEventHandler> logger) : IEventHandler<LaptopPriceChangedEvent>
{
    public async Task HandleAsync(LaptopPriceChangedEvent domainEvent, CancellationToken ct)
    {
        logger.LogInformation(
            "Domain Event Triggered: Laptop {LaptopId} price changed from {OldPrice} to {NewPrice}.", 
            domainEvent.LaptopId, domainEvent.OldPrice, domainEvent.NewPrice);

        try
        {
            if (domainEvent.NewPrice < domainEvent.OldPrice)
            {
                logger.LogInformation("Price dropped for Laptop {LaptopId}! Fetching interested users...", domainEvent.LaptopId);

                await notificationService.NotifyUsersPriceDroppedAsync(domainEvent.LaptopId, domainEvent.NewPrice, ct);

                logger.LogInformation("Price drop notification sent for Laptop {LaptopId}.", domainEvent.LaptopId);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while processing LaptopPriceChangedEvent for Laptop {LaptopId}.", domainEvent.LaptopId);
        }
    }
}
