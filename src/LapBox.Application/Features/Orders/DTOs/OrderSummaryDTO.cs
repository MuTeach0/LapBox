namespace LapBox.Application.Features.Orders.DTOs;

public sealed record OrderSummaryDTO(
    Guid Id,
    DateTimeOffset OrderDate,
    string Status,
    decimal TotalAmount
);