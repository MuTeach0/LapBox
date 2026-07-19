using LapBox.Application.Features.Orders.DTOs;
using LapBox.Domain.Orders;

namespace LapBox.Application.Features.Orders.Mapper;
public static class OrderMapper
{
    public static OrderDetailsDTO ToDetailsDTO(this Order order) => new(
        order.Id,
        order.OrderDate,
        order.Status.ToString(),
        order.CalculateTotal(),
        order.ShippingAddress.Street,
        order.ShippingAddress.City,
        order.ShippingAddress.Country,
        [.. order.OrderItems.Select(item => item.ToDTO())]
    );

    public static OrderItemDTO ToDTO(this OrderItem item) => new(
        item.LaptopId, // أو ProductId حسب المسمى عندك في الـ Domain
        item.Quantity,
        item.UnitPrice
    );

    public static OrderSummaryDTO ToSummaryDTO(this Order order) => new(
        order.Id,
        order.OrderDate,
        order.Status.ToString(),
        order.CalculateTotal()
    );

    public static List<OrderSummaryDTO> ToSummaryDTOs(this IEnumerable<Order> orders) => 
        [.. orders.Select(o => o.ToSummaryDTO())];
}