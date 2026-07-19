using LapBox.Domain.Catalog;

namespace LapBox.Application.Common.Interfaces.Persistence;

public interface ICategoryRepository : IRepository<Category>
{
    Task<bool> ExistsByNameAsync(string name, CancellationToken ct);
    Task<IReadOnlyList<Category>> GetActiveCategoriesAsync(CancellationToken ct = default);
}
