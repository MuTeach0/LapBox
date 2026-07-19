using LapBox.Application.Common.Interfaces.Persistence;
using LapBox.Domain.Catalog;
using Microsoft.EntityFrameworkCore;

namespace LapBox.Infrastructure.Data.Persistence.Repositories;

public sealed class BrandRepository(AppDbContext dbContext)
    : Repository<Brand>(dbContext), IBrandRepository
{
    public async Task<bool> ExistsByNameAsync(string name, CancellationToken ct)
    {
        return await dbContext.Brands.AnyAsync(b => b.Name == name, ct);
    }

    public async Task<IReadOnlyList<Brand>> GetActiveBrandsAsync(CancellationToken ct = default)
    {
        return await dbContext.Brands
            .Where(b => b.IsActive)
            .OrderBy(b => b.Name)
            .ToListAsync(ct);
    }
}
