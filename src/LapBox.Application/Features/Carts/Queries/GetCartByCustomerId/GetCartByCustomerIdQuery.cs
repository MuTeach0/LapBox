using LapBox.Application.Common.Interfaces.Caching;
using LapBox.Application.Features.Carts.DTOs;
using LapBox.Domain.Common.Results;

namespace LapBox.Application.Features.Carts.Queries.GetCartByIdentityId;

public sealed record GetCartByIdentityIdQuery(Guid IdentityId) : ICachedQuery<Result<CartDTO>>
{
    // Cache based on Identity ID - works for both Users and Customers
    public string CacheKey => $"cart_{IdentityId}";

    public string[] Tags => ["carts"];

    public TimeSpan Expiration => TimeSpan.FromMinutes(30); // Cart data changes frequently
}