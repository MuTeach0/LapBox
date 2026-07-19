using LapBox.Application.Common.Interfaces.Events;
using LapBox.Domain.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace LapBox.Infrastructure.Data.Interceptors;

public class DispatchDomainEventsInterceptor(IDomainEventDispatcher dispatcher) : SaveChangesInterceptor
{
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, 
        InterceptionResult<int> result, 
        CancellationToken cancellationToken = default)
    {
        var dbContext = eventData.Context;
        if (dbContext is null) return await base.SavingChangesAsync(eventData, result, cancellationToken);

        // 1. هنجيب كل الـ Entities اللي اتغيرت وفيها Domain Events
        var entities = dbContext.ChangeTracker
            .Entries<AggregateRoot>() // افترضت إن الانتيي اللي فيه الأحداث اسمه AggregateRoot
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();

        // 2. نسحب الأحداث ونفضيها من الـ Entities
        var domainEvents = entities
            .SelectMany(e => e.DomainEvents)
            .ToList();

        entities.ForEach(e => e.ClearDomainEvents());

        // 3. نبعت الأحداث باستخدام الـ Dispatcher بتاعك!
        await dispatcher.DispatchAsync(domainEvents, cancellationToken);

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}