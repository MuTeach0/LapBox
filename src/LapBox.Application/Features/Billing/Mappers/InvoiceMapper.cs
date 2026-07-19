using LapBox.Application.Features.Billing.DTOs;
using LapBox.Domain.Billing;

namespace LapBox.Application.Features.Billing.Mappers;

public static class InvoiceMapper
{
    public static InvoiceDTO ToDTO(this Invoice invoice)
    {
        ArgumentNullException.ThrowIfNull(invoice);

        return new InvoiceDTO
        {
            InvoiceId = invoice.Id,
            OrderId = invoice.OrderId,
            CustomerId = invoice.CustomerId,

            // لو الـ Navigation Property بتاعة الـ Order مشحونة والـ Customer جواها متقفل
            // Customer = invoice.Order?.Customer?.ToDTO(),

            SubTotal = invoice.SubTotal,
            TaxAmount = invoice.TaxAmount,
            TotalAmount = invoice.TotalAmount,
            PaymentStatus = invoice.Status.ToString(),
            PaidDateUtc = invoice.PaidDateUtc,
            // Items = [.. invoice.LineItems.Select(x => x.ToDTO())],
            Items = invoice.LineItems != null
                ? [.. invoice.LineItems.Select(x => x.ToDTO())]
                : []
        };
    }

    public static List<InvoiceDTO> ToDTOs(this IEnumerable<Invoice> entities)
    {
        ArgumentNullException.ThrowIfNull(entities);
        return [.. entities.Select(e => e.ToDTO())];
    }

    public static InvoiceLineItemDTO ToDTO(this InvoiceLineItem item)
    {
        ArgumentNullException.ThrowIfNull(item);

        return new InvoiceLineItemDTO
        {
            InvoiceId = item.InvoiceId,
            LineNumber = item.LineNumber,
            Description = item.Description,
            Quantity = item.Quantity,
            UnitPrice = item.UnitPrice,
            LineTotal = item.LineTotal
        };
    }
     public static List<InvoiceLineItemDTO> ToDTOs(this IEnumerable<InvoiceLineItem> entities) => [.. entities.Select(e => e.ToDTO())];
}