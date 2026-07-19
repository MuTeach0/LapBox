using LapBox.Application.Common.Interfaces.Persistence;
using LapBox.Domain.Laptops;
using Microsoft.EntityFrameworkCore;

namespace LapBox.Infrastructure.Data.Persistence.Repositories;

public sealed class LaptopRepository(AppDbContext dbContext)
    : Repository<Laptop>(dbContext), ILaptopRepository
{
    public Task<bool> ExistsBySkuAsync(string sku, CancellationToken ct) 
        => dbContext.Laptops.AnyAsync(l => l.Sku == sku, ct);


    public async Task<List<Laptop>> GetByIdsAsync(List<Guid> ids, CancellationToken ct = default)
        => await dbContext.Laptops.Where(l => ids.Contains(l.Id)).ToListAsync(ct);

    public async Task<List<Laptop>> GetByIdsWithLockAsync(List<Guid> ids, CancellationToken ct = default)
    {
        if (ids == null || ids.Count == 0) return [];

        // 1. توليد أسماء المعاملات بناءً على عدد الـ Ids (مثل: @p0, @p1, @p2)
        var parameterNames = ids.Select((_, index) => $"@p{index}").ToArray();

        // 2. دمج المعاملات داخل جملة الـ IN
        var inClause = string.Join(", ", parameterNames);

        // 3. كتابة الـ Raw SQL باستخدام صيغة SQL Server الصحيحة (UPDLOCK لمنع الـ Concurrency)
        var sql = $"SELECT * FROM Laptops WITH (UPDLOCK, ROWLOCK) WHERE Id IN ({inClause})";

        // 4. تحويل قائمة الـ Guids إلى مصفوفة Object لتمريرها كمعاملات آمنة
        var parameters = ids.Select(id => (object)id).ToArray();

        return await dbContext.Laptops
            .FromSqlRaw(sql, parameters)
            .ToListAsync(ct);
    }

    public async Task<(List<Laptop> Items, int TotalCount)> GetPagedAsync(
        string? searchTerm,
        Guid? brandId,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var query = dbContext.Laptops.Where(l => l.IsActive);

        if (brandId.HasValue)
            query = query.Where(l => l.BrandId == brandId.Value);

        if (!string.IsNullOrWhiteSpace(searchTerm))
            query = query.Where(l => l.Name.Contains(searchTerm) || l.Description.Contains(searchTerm));

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderBy(l => l.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task<bool> HasActiveLaptopsWithBrandIdAsync(Guid brandId, CancellationToken ct = default)
        => await dbContext.Laptops.AnyAsync(l => l.BrandId == brandId && l.IsActive, ct);
}
