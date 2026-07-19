namespace LapBox.Application.Features.Carts.DTOs;

public sealed record CartItemDTO(
    Guid Id,
    Guid LaptopId,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice);
