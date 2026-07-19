using LapBox.Domain.Carts;

namespace LapBox.Application.Common.Interfaces.Persistence;

public interface ICartRepository : IRepository<Cart>
{
    /// <summary>
    /// Gets a cart by the user's Identity ID.
    /// Works for both Users (no Customer record) and Customers.
    /// </summary>
    Task<Cart?> GetByIdentityIdAsync(Guid identityId, CancellationToken ct = default);

    /// <summary>
    /// Removes a product from all carts that contain it.
    /// </summary>
    Task<IReadOnlyList<Guid>> RemoveProductFromAllCartsAsync(Guid laptopId, CancellationToken ct = default);
    Task ClearCartByIdentityIdAsync(Guid identityId, CancellationToken ct = default);
    void Update(Cart entity, CancellationToken cancellationToken = default);
}