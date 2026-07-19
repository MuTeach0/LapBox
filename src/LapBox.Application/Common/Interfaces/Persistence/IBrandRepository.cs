using LapBox.Domain.Catalog;

namespace LapBox.Application.Common.Interfaces.Persistence;

public interface IBrandRepository : IRepository<Brand>
{
    Task<bool> ExistsByNameAsync(string name, CancellationToken ct);
    Task<IReadOnlyList<Brand>> GetActiveBrandsAsync(CancellationToken ct = default);
}