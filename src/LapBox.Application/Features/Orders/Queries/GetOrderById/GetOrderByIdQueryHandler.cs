using LapBox.Application.Common.Errors;
using LapBox.Application.Common.Interfaces.Persistence;
using LapBox.Application.Features.Orders.DTOs;
using LapBox.Application.Features.Orders.Mapper;
using LapBox.Domain.Common.Results;
using MediatR;

namespace LapBox.Application.Features.Orders.Queries.GetOrderById;

public sealed class GetOrderByIdQueryHandler(IOrderRepository orderRepository)
    : IRequestHandler<GetOrderByIdQuery, Result<OrderDetailsDTO>>
{
    public async Task<Result<OrderDetailsDTO>> Handle(GetOrderByIdQuery query, CancellationToken ct)
    {
        // جلب الطلب مع المنتجات لضمان عدم حدوث Lazy Loading Exception
        var order = await orderRepository.GetOrderWithItemsAsync(query.OrderId, ct);

        if (order is null) return ApplicationErrors.OrderNotFound;

        if (order.UserId != query.CurrentUserId && query.CurrentUserRole != "Admin")
            return ApplicationErrors.OrderUnauthorized;

        return order.ToDetailsDTO();
    }
}