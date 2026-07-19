namespace LapBox.Application.Features.Billing.DTOs;

public sealed class InvoicePdfDTO
{
    public byte[]? Content { get; init; }
    public string? FileName { get; init; }
    public string? ContentType { get; init; } = "application/pdf";
}