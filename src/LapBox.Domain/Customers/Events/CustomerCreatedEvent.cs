using LapBox.Domain.Common;

namespace LapBox.Domain.Customers.Events;

public sealed record CustomerCreatedEvent(Guid CustomerId, Guid IdentityId, string Email) : DomainEvent;