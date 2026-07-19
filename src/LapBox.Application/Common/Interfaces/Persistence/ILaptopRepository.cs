using LapBox.Domain.Laptops;

namespace LapBox.Application.Common.Interfaces.Persistence;

public interface ILaptopRepository : IRepository<Laptop>
{
    Task<List<Laptop>> GetByIdsAsync(List<Guid> ids, CancellationToken ct = default);

    /// <summary>
    /// Locks the rows for update (SELECT ... FOR UPDATE) to prevent concurrent inventory modifications.
    /// Must be called inside a transaction.
    /// </summary>
    Task<List<Laptop>> GetByIdsWithLockAsync(List<Guid> ids, CancellationToken ct = default);
    Task<(List<Laptop> Items, int TotalCount)> GetPagedAsync(
        string? searchTerm,
        Guid? brandId,
        int page,
        int pageSize,
        CancellationToken ct = default);
    Task<bool> HasActiveLaptopsWithBrandIdAsync(Guid brandId, CancellationToken ct = default);
    Task<bool> ExistsBySkuAsync(string sku, CancellationToken ct);
}