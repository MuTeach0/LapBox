using LapBox.Application.Common.Interfaces.Persistence;
using LapBox.Domain.Promotions;
using Microsoft.EntityFrameworkCore;

namespace LapBox.Infrastructure.Data.Persistence.Repositories;

public sealed class PromotionRepository(AppDbContext dbContext)
    : Repository<Promotion>(dbContext), IPromotionRepository
{
    public async Task<Promotion?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await dbContext.Promotions
            .FirstOrDefaultAsync(p => p.Code == code, cancellationToken);
    }
}
