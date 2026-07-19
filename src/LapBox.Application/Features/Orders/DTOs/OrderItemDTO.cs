namespace LapBox.Application.Features.Orders.DTOs;

public record OrderItemDTO(Guid ProductId, int Quantity, decimal UnitPrice);