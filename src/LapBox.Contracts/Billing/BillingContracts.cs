namespace LapBox.Contracts.Billing;

public sealed record InvoiceLineItemResponse(
    Guid InvoiceId,
    int LineNumber,
    string? Description,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal);

public sealed record CustomerInvoiceInfo(
    string Name,
    string Email,
    string? PhoneNumber);

public sealed record InvoiceResponse(
    Guid InvoiceId,
    Guid OrderId,
    Guid CustomerId,
    CustomerInvoiceInfo? Customer,
    decimal SubTotal,
    decimal TaxAmount,
    decimal TotalAmount,
    string Status,
    DateTimeOffset? PaidDateUtc,
    IReadOnlyList<InvoiceLineItemResponse> Items);

public sealed record MarkInvoiceAsPaidRequest();

public sealed record GetInvoicePdfRequest();
