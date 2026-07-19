using LapBox.Domain.Common;

namespace LapBox.Domain.Orders.Events;

// public sealed record OrderCreatedEvent(Guid OrderId, Guid UserId) : DomainEvent;
// public sealed record OrderCreatedEvent(Guid OrderId, Guid IdentityId, Guid CustomerId, List<OrderItemPayload> Items) : DomainEvent;

public sealed record OrderCreatedDomainEvent(
    Guid OrderId,
    Guid UserId,
    List<OrderItemPayload> Items) : DomainEvent;