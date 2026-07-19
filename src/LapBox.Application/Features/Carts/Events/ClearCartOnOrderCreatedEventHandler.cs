using LapBox.Application.Common.Events;
using LapBox.Application.Features.Carts.Command.ClearCart;
using LapBox.Domain.Orders.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LapBox.Application.Features.Carts.Events;

public sealed class ClearCartOnOrderCreatedEventHandler(
    ISender sender,
    ILogger<ClearCartOnOrderCreatedEventHandler> logger) : IEventHandler<OrderCreatedDomainEvent>
{
    public async Task HandleAsync(OrderCreatedDomainEvent domainEvent, CancellationToken ct)
    {
        logger.LogInformation("Received OrderCreatedEvent for Order: {OrderId}. Triggering cart clearance for User: {UserId}", 
            domainEvent.OrderId, domainEvent.UserId);

        var result = await sender.Send(new ClearCartCommand(domainEvent.UserId, CheckedOut: true), ct);

        if (result.IsError)
        {
            logger.LogError("Failed to clear cart for User: {UserId} after order creation. Error: {ErrorDescription}", 
                domainEvent.UserId, result.TopError.Description);
        }
        else
        {
            logger.LogInformation("Successfully cleared cart for User: {UserId} via OrderCreatedEvent", 
                domainEvent.UserId);
        }
    }
}
