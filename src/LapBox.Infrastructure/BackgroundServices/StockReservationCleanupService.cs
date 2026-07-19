using LapBox.Application.Common.Interfaces.Persistence;
using LapBox.Domain.Laptops;
using LapBox.Infrastructure.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LapBox.Infrastructure.BackgroundServices;

/// <summary>
/// Periodically scans for expired StockReservations and restores inventory + releases slots.
/// Runs every minute. Expired = Active status but ExpiresAtUtc < now.
/// </summary>
public sealed class StockReservationCleanupService(
    IServiceScopeFactory scopeFactory,
    ILogger<StockReservationCleanupService> logger,
    IOptions<AppSettings> options)
    : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly ILogger<StockReservationCleanupService> _logger = logger;
    private readonly AppSettings _appSettings = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("StockReservationCleanupService started");

        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(_appSettings.ReservationCleanupIntervalMinutes));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                await CleanupExpiredReservationsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during StockReservation cleanup cycle");
            }

            // Run every _appSettings.ReservationCleanupIntervalMinutes seconds
            await Task.Delay(TimeSpan.FromMinutes(_appSettings.ReservationCleanupIntervalMinutes), stoppingToken);
        }
    }

    private async Task CleanupExpiredReservationsAsync(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var reservationRepo = scope.ServiceProvider.GetRequiredService<IStockReservationRepository>();
        var laptopRepo = scope.ServiceProvider.GetRequiredService<ILaptopRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var expired = await reservationRepo.GetExpiredActiveReservationsAsync(ct);

        if (expired.Count == 0) return;

        _logger.LogInformation("Found {Count} expired stock reservations to clean up", expired.Count);

        // Group by laptopId so we can restore inventory in bulk
        var grouped = expired
            .Where(r => r.IsExpired())
            .GroupBy(r => r.LaptopId)
            .ToList();

        foreach (var group in grouped)
        {
            var laptopId = group.Key;
            var totalQuantity = group.Sum(r => r.Quantity);

            var laptop = await laptopRepo.GetByIdAsync(laptopId, ct);
            if (laptop is not null)
            {
                laptop.UpdateInventory(totalQuantity); // Restore inventory
                _logger.LogInformation(
                    "Restoring {Quantity} units to Laptop {LaptopId}. New inventory: {Inventory}",
                    totalQuantity, laptopId, laptop.InventoryQuantity);
            }
        }

        // Mark all expired as Released
        foreach (var reservation in expired.Where(r => r.IsExpired()))
        {
            reservation.Release();
        }

        await unitOfWork.SaveChangesAsync(ct);

        _logger.LogInformation(
            "StockReservation cleanup complete: {Count} reservations released, inventory restored",
            expired.Count);
    }
}
