namespace LapBox.Application.Features.Orders.DTOs;

public record OrderDetailsDTO(
    Guid Id,
    DateTimeOffset OrderDate,
    string Status,
    decimal TotalAmount,
    string ShippingStreet,
    string ShippingCity,
    string ShippingCountry,
    List<OrderItemDTO> Items
);