using LapBox.Domain.Common.Results;

namespace LapBox.Domain.Billing;

public static class InvoiceErrors
{
    public static readonly Error NotFound =
        Error.NotFound("Invoice.NotFound", "The requested invoice was not found.");

    public static readonly Error AlreadyPaid =
        Error.Conflict("Invoice.Status", "Invoice is already paid.");

    public static readonly Error LineItemsRequired =
        Error.Validation("Invoice.LineItems", "Invoice must contain at least one line item.");

    public static readonly Error InvalidStatusForDiscount =
        Error.Conflict("Invoice.Discount", "Discount can only be applied to unpaid invoices.");

    public static readonly Error DiscountAmountInvalid =
        Error.Validation("Invoice.Discount", "Discount amount must be greater than zero.");

    public static readonly Error DiscountExceedsSubTotal =
        Error.Validation("Invoice.Discount", "Discount amount cannot exceed the invoice subtotal.");

    public static Error QuantityInvalid(int lineNumber) =>
        Error.Validation("Invoice.LineItems.Quantity", $"Quantity must be greater than zero at line {lineNumber}.");

    public static Error UnitPriceInvalid(int lineNumber) =>
        Error.Validation("Invoice.LineItems.UnitPrice", $"Unit price must be greater than zero at line {lineNumber}.");
}