using LapBox.Domain.Common;
using LapBox.Domain.Orders.ValueObjects;

namespace LapBox.Domain.Orders.Events;

public sealed record ShipmentRequestedEvent(
    Guid OrderId,
    Guid IdentityId,
    ShippingAddress ShippingAddress) : DomainEvent;
