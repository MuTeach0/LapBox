using LapBox.Domain.Common;

namespace LapBox.Domain.Payments.Events;

public record PaymentFailedEvent(Guid PaymentId, Guid OrderId, string Reason) : DomainEvent;