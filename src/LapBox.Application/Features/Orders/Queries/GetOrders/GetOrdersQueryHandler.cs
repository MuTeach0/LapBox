using LapBox.Application.Common.Errors;
using LapBox.Application.Common.Interfaces.Persistence;
using LapBox.Application.Common.Models;
using LapBox.Application.Features.Orders.DTOs;
using LapBox.Application.Features.Orders.Mapper;
using LapBox.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LapBox.Application.Features.Orders.Queries.GetOrders;

public sealed class GetOrdersQueryHandler(
    ILogger<GetOrdersQueryHandler> logger,
    IOrderRepository orderRepository)
    : IRequestHandler<GetOrdersQuery, Result<PaginatedList<OrderSummaryDTO>>>
{
    public async Task<Result<PaginatedList<OrderSummaryDTO>>> Handle(GetOrdersQuery query, CancellationToken ct)
    {
        logger.LogInformation("Admin/Staff User {CurrentUserId} is requesting the global orders list.", query.CurrentUserId);

        // 🛡️ جدار الحماية (Role-Based Authorization):
        // الـ Query دي مخصصة حصرياً للـ Admin أو الـ Staff.. لو عميل عادي حاول يوصل لها بنفرمل الريكويست فوراً
        if (query.CurrentUserRole != "Admin" && query.CurrentUserRole != "Staff")
        {
            logger.LogWarning("Security Alert! Customer {CurrentUserId} tried to access global admin orders list.", query.CurrentUserId);
            return ApplicationErrors.OrderUnauthorized;
        }

        var (orders, totalCount) = await orderRepository.GetAllPaginatedAsync(query.Page, query.PageSize, ct);
        var dtos = orders.ToSummaryDTOs();

        return new PaginatedList<OrderSummaryDTO>
        {
            Items = dtos,
            PageNumber = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize)
        };
    }
}