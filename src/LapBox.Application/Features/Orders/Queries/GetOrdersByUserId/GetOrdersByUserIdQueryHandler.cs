using LapBox.Application.Common.Errors;
using LapBox.Application.Common.Interfaces.Persistence;
using LapBox.Application.Features.Orders.DTOs;
using LapBox.Application.Features.Orders.Mapper;
using LapBox.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LapBox.Application.Features.Orders.Queries.GetOrdersByUserId;

public sealed class GetOrdersByUserIdQueryHandler(
    ILogger<GetOrdersByUserIdQueryHandler> logger, IOrderRepository orderRepository)
    : IRequestHandler<GetOrdersByUserIdQuery, Result<List<OrderSummaryDTO>>>
{
    public async Task<Result<List<OrderSummaryDTO>>> Handle(GetOrdersByUserIdQuery query, CancellationToken ct)
    {
        logger.LogInformation("User {CurrentUserId} with role {Role} is requesting order history for User {TargetUserId}", 
            query.CurrentUserId, query.CurrentUserRole, query.UserId);

        if (query.UserId != query.CurrentUserId && query.CurrentUserRole != "Admin")
        {
            logger.LogWarning("Unauthorized data access attempt! User {CurrentUserId} tried to view orders of User {TargetUserId}", 
                query.CurrentUserId, query.UserId);
            return ApplicationErrors.OrderUnauthorized;
        }
        var orders = await orderRepository.GetByUserIdAsync(query.UserId, ct);

        // استخدام الـ Mapper لتحويل الـ Domain Entities إلى DTOs
        return orders.ToSummaryDTOs(); 
    }
}