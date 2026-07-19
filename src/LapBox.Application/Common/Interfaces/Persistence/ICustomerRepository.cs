using LapBox.Domain.Customers;

namespace LapBox.Application.Common.Interfaces.Persistence;

public interface ICustomerRepository : IRepository<Customer>
{
    Task<Customer?> GetByIdentityIdAsync(Guid identityId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByEmailAsync(string email, CancellationToken ct);
    Task<Customer?> GetProfileAsync(Guid id, CancellationToken ct = default);
    Task<(List<Customer> Items, int TotalCount)> GetAllPaginatedAsync(int page, int pageSize, CancellationToken ct);
}