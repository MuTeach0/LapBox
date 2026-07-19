using LapBox.Domain.Common.Results;

namespace LapBox.Domain.Payments;

public static class PaymentErrors
{
    public static readonly Error NotPending = 
        Error.Validation("Payment.NotPending", "Only pending payments can be processed.");

    public static readonly Error AmountInvalid = 
        Error.Validation("Payment.Amount", "Payment amount must be greater than zero.");
}