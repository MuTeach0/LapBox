using LapBox.Domain.Common;
using LapBox.Domain.Orders.Enums;

namespace LapBox.Domain.Orders.Events;

public sealed record OrderStatusChangedDomainEvent(
    Guid OrderId, 
    OrderStatus NewStatus, 
    string? TrackingLabel = null) : DomainEvent;