using LapBox.Domain.Common.Results;
using MediatR;

namespace LapBox.Application.Features.Carts.Command.ClearCart;

public sealed record ClearCartCommand(Guid IdentityId, bool CheckedOut = false) : IRequest<Result<Success>>;
