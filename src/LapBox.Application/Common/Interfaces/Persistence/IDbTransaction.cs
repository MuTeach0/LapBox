namespace LapBox.Application.Common.Interfaces.Persistence;

/// <summary>
/// Represents a database transaction that can be committed or rolled back.
/// This is a wrapper around EF Core's IDbContextTransaction to avoid 
/// Application layer depending on Infrastructure/EF Core.
/// </summary>
public interface IDbTransaction : IAsyncDisposable
{
    Task CommitAsync(CancellationToken cancellationToken = default);
    Task RollbackAsync(CancellationToken cancellationToken = default);
}
