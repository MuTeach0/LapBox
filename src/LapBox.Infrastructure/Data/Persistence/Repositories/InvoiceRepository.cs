using LapBox.Application.Common.Interfaces.Persistence;
using LapBox.Domain.Billing;
using Microsoft.EntityFrameworkCore;

namespace LapBox.Infrastructure.Data.Persistence.Repositories;

public sealed class InvoiceRepository(AppDbContext dbContext)
    : Repository<Invoice>(dbContext), IInvoiceRepository
{
    public async Task<Invoice?> GetByOrderIdAsync(Guid orderId, CancellationToken ct)
    {
        return await dbContext.Invoices
            .Include(i => i.LineItems)
            .FirstOrDefaultAsync(i => i.OrderId == orderId, ct);
    }
}
