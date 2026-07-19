using LapBox.Domain.Common;
using LapBox.Domain.Orders.ValueObjects;

namespace LapBox.Application.Features.Billing.Events;

public record ShipmentRequestedEvent(
    Guid OrderId, 
    Guid IdentityId, 
    ShippingAddress ShippingAddress) : DomainEvent;