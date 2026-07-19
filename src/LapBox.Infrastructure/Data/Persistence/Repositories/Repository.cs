using LapBox.Application.Common.Interfaces.Persistence;
using LapBox.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace LapBox.Infrastructure.Data.Persistence.Repositories;

public abstract class Repository<T>(AppDbContext dbContext) : IRepository<T>
    where T : AggregateRoot
{
    public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await dbContext.Set<T>().FindAsync([id], cancellationToken);

    public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default) =>
        await dbContext.Set<T>().ToListAsync(ct);

    public async Task AddAsync(T entity, CancellationToken cancellationToken = default) =>
        await dbContext.Set<T>().AddAsync(entity, cancellationToken);

    public void Remove(T entity) =>
        dbContext.Set<T>().Remove(entity);
}