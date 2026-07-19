using LapBox.Application.Common.Interfaces.Persistence;
using LapBox.Domain.Catalog;
using Microsoft.EntityFrameworkCore;

namespace LapBox.Infrastructure.Data.Persistence.Repositories;

public sealed class CategoryRepository(AppDbContext dbContext)
    : Repository<Category>(dbContext), ICategoryRepository
{
    public async Task<bool> ExistsByNameAsync(string name, CancellationToken ct)
    {
        return await dbContext.Set<Category>().AnyAsync(c => c.Name == name, ct);
    }

    public async Task<IReadOnlyList<Category>> GetActiveCategoriesAsync(CancellationToken ct = default)
    {
        return await dbContext.Set<Category>()
            .Where(c => c.IsActive)
            .ToListAsync(ct);
    }
}
