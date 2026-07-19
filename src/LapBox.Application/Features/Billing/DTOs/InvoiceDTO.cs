using LapBox.Application.Features.Customers.DTOs;

namespace LapBox.Application.Features.Billing.DTOs;

public class InvoiceDTO
{
    public Guid InvoiceId { get; set; }
    public Guid OrderId { get; set; }
    public Guid CustomerId { get; set; }
    public CustomerDTO? Customer { get; set; } // بيانات العميل اللي اشترى اللابتوب

    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string? PaymentStatus { get; set; }
    public DateTimeOffset? PaidDateUtc { get; set; }

    // لستة الأجهزة والقطع اللي جوه الفاتورة
    public List<InvoiceLineItemDTO> Items { get; set; } = [];
}