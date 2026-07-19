namespace LapBox.Application.Common.Interfaces.Persistence;

public interface IUnitOfWork
{
    // دالة لحفظ كل التغييرات التي تمت على الـ Aggregates
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Begins a database transaction for operations that require atomicity.
    /// Use with 'await using var transaction = await unitOfWork.BeginTransactionAsync()'
    /// </summary>
    Task<IDbTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
}