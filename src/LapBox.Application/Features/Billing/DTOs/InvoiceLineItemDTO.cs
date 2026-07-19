namespace LapBox.Application.Features.Billing.DTOs;

public record InvoiceLineItemDTO
{
    public Guid InvoiceId { get; set; }
    public int LineNumber { get; set; }
    public string? Description { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}