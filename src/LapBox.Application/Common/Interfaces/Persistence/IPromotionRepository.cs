using LapBox.Domain.Promotions;

namespace LapBox.Application.Common.Interfaces.Persistence;

public interface IPromotionRepository : IRepository<Promotion>
{
    Task<Promotion?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
}