using LapBox.Domain.Common.Results;
using MediatR;

namespace LapBox.Application.Features.Orders.Command.RemoveOrder;

public sealed record RemoveOrderCommand(
    Guid OrderId, 
    Guid CurrentUserId,    // الـ User ID المأخوذ من الـ Token بأمان
    string CurrentUserRole // الـ Role (Admin أو Customer) المأخوذة من الـ Token
) : IRequest<Result<Deleted>>;