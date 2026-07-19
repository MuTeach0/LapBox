using LapBox.Application.Common.Events;
using LapBox.Domain.Orders.Events;
using Microsoft.Extensions.Logging;

namespace LapBox.Application.Features.Shipping.Events;

/// <summary>
/// Temporary shipping-adapter simulation. Replace this handler with an HTTP/queue adapter
/// when the shipping provider contract is available.
/// </summary>
public sealed class MockShipmentRequestedEventHandler(
    ILogger<MockShipmentRequestedEventHandler> logger) : IEventHandler<ShipmentRequestedEvent>
{
    public Task HandleAsync(ShipmentRequestedEvent domainEvent, CancellationToken ct)
    {
        logger.LogInformation(
            "[MOCK SHIPPING] Shipment requested for order {OrderId}, identity {IdentityId}, destination: {Street}, {City}, {Country}",
            domainEvent.OrderId,
            domainEvent.IdentityId,
            domainEvent.ShippingAddress.Street,
            domainEvent.ShippingAddress.City,
            domainEvent.ShippingAddress.Country);

        return Task.CompletedTask;
    }
}
