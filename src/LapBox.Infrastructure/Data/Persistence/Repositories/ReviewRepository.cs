using LapBox.Application.Common.Interfaces.Persistence;
using LapBox.Domain.Reviews;
using Microsoft.EntityFrameworkCore;

namespace LapBox.Infrastructure.Data.Persistence.Repositories;

public sealed class ReviewRepository(AppDbContext dbContext)
    : Repository<Review>(dbContext), IReviewRepository
{
    public async Task<IReadOnlyCollection<Review>> GetByLaptopIdAsync(Guid laptopId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Reviews
            .Where(r => r.LaptopId == laptopId)
            .OrderByDescending(r => r.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }
}
