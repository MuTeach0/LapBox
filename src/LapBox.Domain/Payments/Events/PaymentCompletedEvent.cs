using LapBox.Domain.Common;

namespace LapBox.Domain.Payments.Events;

public record PaymentCompletedEvent(Guid PaymentId, Guid OrderId, string ExternalTransactionId) : DomainEvent;