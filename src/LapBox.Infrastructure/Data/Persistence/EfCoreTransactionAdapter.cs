using LapBox.Application.Common.Interfaces.Persistence;
using Microsoft.EntityFrameworkCore.Storage;

namespace LapBox.Infrastructure.Data.Persistence;

/// <summary>
/// Adapter that wraps EF Core's IDbContextTransaction and exposes it as the Application layer's IDbTransaction interface
/// </summary>
public sealed class EfCoreTransactionAdapter(IDbContextTransaction efTransaction) : IDbTransaction
{
    public Task CommitAsync(CancellationToken cancellationToken = default)
        => efTransaction.CommitAsync(cancellationToken);

    public Task RollbackAsync(CancellationToken cancellationToken = default)
        => efTransaction.RollbackAsync(cancellationToken);

    public async ValueTask DisposeAsync()
    {
        await efTransaction.DisposeAsync();
    }
}
