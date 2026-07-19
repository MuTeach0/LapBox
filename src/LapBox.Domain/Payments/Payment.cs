using LapBox.Domain.Common;
using LapBox.Domain.Common.Results;
using LapBox.Domain.Payments.Enums;
using LapBox.Domain.Payments.Events;

namespace LapBox.Domain.Payments;

public sealed class Payment : AggregateRoot
{
    public Guid OrderId { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = "EGP";
    public PaymentStatus Status { get; private set; }
    public string? ExternalTransactionId { get; private set; } // المعاملة في Paymob
    public string? FailureReason { get; private set; }

    private Payment() { }

    private Payment(Guid id, Guid orderId, decimal amount) : base(id)
    {
        OrderId = orderId;
        Amount = amount;
        Status = PaymentStatus.Pending;
    }

    public static Result<Payment> Initiate(Guid orderId, decimal amount)
    {
        if (orderId == Guid.Empty) return Error.Validation("Payment.OrderId", "Order ID is required.");
        if (amount <= 0) return PaymentErrors.AmountInvalid;

        return new Payment(Guid.NewGuid(), orderId, amount);
    }

    public Result<Success> Complete(string externalTransactionId)
    {
        if (Status != PaymentStatus.Pending) return PaymentErrors.NotPending;

        Status = PaymentStatus.Success;
        ExternalTransactionId = externalTransactionId;

        AddDomainEvent(new PaymentCompletedEvent(Id, OrderId, externalTransactionId));
        return Result.Success;
    }

    public Result<Success> Fail(string reason)
    {
        if (Status != PaymentStatus.Pending) return PaymentErrors.NotPending;

        Status = PaymentStatus.Failed;
        FailureReason = reason;

        AddDomainEvent(new PaymentFailedEvent(Id, OrderId, reason));
        return Result.Success;
    }
}