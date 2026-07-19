using LapBox.Domain.Common;

namespace LapBox.Application.Common.Interfaces.Persistence;

public interface IRepository<T> where T : AggregateRoot
{
    // سنضع هنا العمليات المشتركة فقط
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(T entity, CancellationToken cancellationToken = default);
    void Remove(T entity);
}