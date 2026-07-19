using LapBox.Domain.Common;

namespace LapBox.Domain.Orders.Events;

public sealed record OrderCancelledEvent(Guid OrderId, Guid UserId) : DomainEvent;
