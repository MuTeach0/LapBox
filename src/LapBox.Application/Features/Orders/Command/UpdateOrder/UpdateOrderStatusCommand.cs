using LapBox.Domain.Common.Results;
using LapBox.Domain.Orders.Enums;
using MediatR;

namespace LapBox.Application.Features.Orders.Command.UpdateOrder;

public record UpdateOrderStatusCommand(
    Guid OrderId,
    OrderStatus NewStatus,
    string? TrackingLabel = null) : IRequest<Result<Updated>>;