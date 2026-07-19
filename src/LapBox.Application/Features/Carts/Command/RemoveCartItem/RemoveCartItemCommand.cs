using LapBox.Domain.Common.Results;
using MediatR;

namespace LapBox.Application.Features.Carts.Command.RemoveCartItem;
public sealed record RemoveCartItemCommand(Guid IdentityId, Guid LaptopId) : IRequest<Result<Success>>;
