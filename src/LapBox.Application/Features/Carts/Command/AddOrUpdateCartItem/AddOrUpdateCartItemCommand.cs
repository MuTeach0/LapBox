using LapBox.Domain.Common.Results;
using MediatR;

namespace LapBox.Application.Features.Carts.Command.AddOrUpdateCartItem;

public record AddOrUpdateCartItemCommand(
    Guid IdentityId,
    Guid LaptopId,
    int Quantity,
    decimal UnitPrice) : IRequest<Result<Success>>;
