using System.Reflection;
using FluentValidation;
using LapBox.Application.Common.Behaviors;
using LapBox.Application.Common.Events;
using LapBox.Application.Common.Interfaces.Events;
using Microsoft.Extensions.Caching.Hybrid;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // FluentValidation — scans for all AbstractValidator<T> in the assembly
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // Hybrid cache (in-memory + distributed) — used by CachingBehavior
        services.AddHybridCache();

        // Domain Event Dispatcher
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

        // Auto-register all IEventHandler implementations
        var handlerTypes = Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => t.GetInterfaces().Any(i => 
                i.IsGenericType && 
                i.GetGenericTypeDefinition() == typeof(IEventHandler<>)))
            .ToList();

        foreach (var handlerType in handlerTypes)
        {
            var handlerInterface = handlerType.GetInterfaces()
                .First(i => i.IsGenericType && 
                    i.GetGenericTypeDefinition() == typeof(IEventHandler<>));
            
            services.AddScoped(handlerInterface, handlerType);
        }

        // MediatR pipeline behaviors
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            cfg.AddOpenBehavior(typeof(PerformanceBehavior<,>));
            cfg.AddOpenBehavior(typeof(UnhandledExceptionBehavior<,>));
            cfg.AddOpenBehavior(typeof(CachingBehavior<,>));
        });

        return services;
    }
}
