using LapBox.Application.Common.Interfaces.Persistence;
using LapBox.Domain.Carts;
using Microsoft.EntityFrameworkCore;

namespace LapBox.Infrastructure.Data.Persistence.Repositories;

public sealed class CartRepository(AppDbContext dbContext)
    : Repository<Cart>(dbContext), ICartRepository
{
    public async Task<Cart?> GetByIdentityIdAsync(Guid identityId, CancellationToken ct = default)
    {
        return await dbContext.Carts
            .Include(c => c.Items)
            //.AsNoTracking()
            .FirstOrDefaultAsync(c => c.IdentityId == identityId, ct);
    }

    public async Task<IReadOnlyList<Guid>> RemoveProductFromAllCartsAsync(Guid laptopId, CancellationToken ct = default)
    {
        var cartsWithProduct = await dbContext.Carts
            .Include(c => c.Items)
            .Where(c => c.Items.Any(i => i.LaptopId == laptopId))
            .ToListAsync(ct);

        var affectedIdentityIds = new List<Guid>();

        foreach (var cart in cartsWithProduct)
        {
            var removedItems = cart.Items.Where(i => i.LaptopId == laptopId).ToList();
            foreach (var item in removedItems)
            {
                cart.RemoveItem(item.LaptopId);
            }
            affectedIdentityIds.Add(cart.IdentityId);
        }

        return affectedIdentityIds;
    }

    public async Task ClearCartByIdentityIdAsync(Guid identityId, CancellationToken ct = default)
    {
        var cart = await dbContext.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.IdentityId == identityId, ct);

        cart?.Clear();
    }

    public void Update(Cart entity, CancellationToken cancellationToken = default) =>
        dbContext.Carts.Update(entity);
}
