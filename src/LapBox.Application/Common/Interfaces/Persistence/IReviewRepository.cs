using LapBox.Domain.Reviews;

namespace LapBox.Application.Common.Interfaces.Persistence;

public interface IReviewRepository : IRepository<Review>
{
    Task<IReadOnlyCollection<Review>> GetByLaptopIdAsync(Guid laptopId, CancellationToken cancellationToken = default);
}