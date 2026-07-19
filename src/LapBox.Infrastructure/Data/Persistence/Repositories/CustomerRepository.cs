using LapBox.Application.Common.Interfaces.Persistence;
using LapBox.Domain.Customers;
using Microsoft.EntityFrameworkCore;

namespace LapBox.Infrastructure.Data.Persistence.Repositories;

public sealed class CustomerRepository(AppDbContext dbContext) 
    : Repository<Customer>(dbContext), ICustomerRepository
{
    public async Task<Customer?> GetByIdentityIdAsync(Guid identityId, CancellationToken cancellationToken = default)
    {
        // افترضنا أن عندك خاصية IdentityId للربط مع جدول الـ Users الأساسي في الـ Auth
        return await dbContext.Customers
            .FirstOrDefaultAsync(c => EF.Property<Guid>(c, "IdentityId") == identityId, cancellationToken); 
    }

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken ct)
    {
        return await dbContext.Customers.AnyAsync(c => c.Email == email, ct);
    }

    public async Task<Customer?> GetProfileAsync(Guid id, CancellationToken ct = default)
    {
        // جلب العميل مع العناوين الخاصة به (بما إن الـ Addresses جزء من الـ Customer Aggregate)
        return await dbContext.Customers
            .Include(c => c.Addresses) 
            .FirstOrDefaultAsync(c => c.Id == id, ct);
    }

    public async Task<(List<Customer> Items, int TotalCount)> GetAllPaginatedAsync(
        int page, int pageSize, CancellationToken ct)
    {
        var query = dbContext.Customers;

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderBy(c => c.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }
}