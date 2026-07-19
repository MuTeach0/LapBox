using LapBox.Domain.Billing;

namespace LapBox.Application.Common.Interfaces.Persistence;

public interface IInvoiceRepository : IRepository<Invoice>
{
    Task<Invoice?> GetByOrderIdAsync(Guid orderId, CancellationToken ct);
}