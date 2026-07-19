using LapBox.Application.Common.Interfaces.Identity;
using LapBox.Domain.Common;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace LapBox.Infrastructure.Data.Interceptors;

public class AuditableEntityInterceptor(IUser user, TimeProvider dateTime) : SaveChangesInterceptor
{
    private readonly IUser _user = user;
    private readonly TimeProvider _dateTime = dateTime;

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        UpdateEntities(eventData.Context);

        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        UpdateEntities(eventData.Context);

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public void UpdateEntities(DbContext? context)
    {
        if (context is null) return;

        foreach (var entry in context.ChangeTracker.Entries<AuditableEntity>())
        {
            if (entry.State is EntityState.Added or EntityState.Modified || entry.HasChangedOwnedEntities())
            {
                var utcNow = _dateTime.GetUtcNow();

                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedBy = _user.Id?.ToString()?? "System";
                    entry.Entity.CreatedAtUtc = utcNow;
                    // entry.Property("CreatedBy").CurrentValue = _user.Id.ToString();
                    // entry.Property("CreatedAtUtc").CurrentValue = utcNow;
                }
                entry.Entity.LastModifiedBy = _user.Id?.ToString()?? "System";
                entry.Entity.LastModifiedUtc = utcNow;
                // entry.Property("LastModifiedBy").CurrentValue = _user.Id.ToString();
                // entry.Property("LastModifiedUtc").CurrentValue = utcNow;

                foreach (var ownedEntry in entry.References)
                {
                    if (ownedEntry.TargetEntry is { Entity: AuditableEntity ownedEntity } && ownedEntry.TargetEntry.State is EntityState.Added or EntityState.Modified)
                    {
                        // if (ownedEntry.TargetEntry.State == EntityState.Added)
                        // {
                        //     ownedEntry.TargetEntry.Property("CreatedBy").CurrentValue = _user.Id.ToString();
                        //     ownedEntry.TargetEntry.Property("CreatedAtUtc").CurrentValue = utcNow;
                        // }

                        // ownedEntry.TargetEntry.Property("LastModifiedBy").CurrentValue = _user.Id.ToString();
                        // ownedEntry.TargetEntry.Property("LastModifiedUtc").CurrentValue = utcNow;

                        if (ownedEntry.TargetEntry.State == EntityState.Added)
                        {
                            ownedEntity.CreatedBy = _user.Id?.ToString()?? "System";
                            ownedEntity.CreatedAtUtc = utcNow;
                        }

                        ownedEntity.LastModifiedBy = _user.Id?.ToString()?? "System";
                        ownedEntity.LastModifiedUtc = utcNow;
                    }
                }
            }
        }
    }
}

public static class Extensions
{
    public static bool HasChangedOwnedEntities(this EntityEntry entry) =>
        entry.References.Any(r =>
            r.TargetEntry?.Metadata.IsOwned() == true &&
            (r.TargetEntry.State == EntityState.Added || r.TargetEntry.State == EntityState.Modified));
}