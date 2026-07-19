using LapBox.Application.Common.Interfaces.Persistence;
using LapBox.Application.Features.Carts.DTOs;
using LapBox.Application.Features.Carts.Mappers;
using LapBox.Domain.Carts;
using LapBox.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LapBox.Application.Features.Carts.Queries.GetCartByIdentityId;

public sealed class GetCartByIdentityIdQueryHandler(
    ICartRepository cartRepository,
    ILogger<GetCartByIdentityIdQueryHandler> logger) : IRequestHandler<GetCartByIdentityIdQuery, Result<CartDTO>>
{
    public async Task<Result<CartDTO>> Handle(GetCartByIdentityIdQuery query, CancellationToken ct)
    {
        logger.LogInformation("Cache Miss! Fetching cart from Database for Identity: {IdentityId}", query.IdentityId);

        var cart = await cartRepository.GetByIdentityIdAsync(query.IdentityId, ct);

        if (cart is null)
        {
            logger.LogWarning("Cart not found for Identity: {IdentityId}", query.IdentityId);
            return new CartDTO(Guid.Empty, query.IdentityId, "Active", [], 0);
        }

        return cart.ToDTO();
    }
}
