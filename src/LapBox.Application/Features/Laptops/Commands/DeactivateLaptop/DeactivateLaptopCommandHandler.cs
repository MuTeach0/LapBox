using LapBox.Application.Common.Errors;
using LapBox.Application.Common.Interfaces.Persistence;
using LapBox.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace LapBox.Application.Features.Laptops.Commands.DeactivateLaptop;

public class DeactivateLaptopCommandHandler(
    ILogger<DeactivateLaptopCommandHandler> logger,
    ILaptopRepository laptopRepository,
    IUnitOfWork unitOfWork,
    HybridCache cache) : IRequestHandler<DeactivateLaptopCommand, Result<Success>>
{
    public async Task<Result<Success>> Handle(DeactivateLaptopCommand command, CancellationToken ct)
    {
        logger.LogInformation("Attempting to deactivate Laptop ID: {LaptopId}", command.LaptopId);

        // 1. جلب اللابتوب من قاعدة البيانات
        var laptop = await laptopRepository.GetByIdAsync(command.LaptopId, ct);
        if (laptop is null)
        {
            logger.LogWarning("Deactivation failed. Laptop {LaptopId} not found.", command.LaptopId);
            return ApplicationErrors.LaptopNotFound;
        }

        // 2. استدعاء بزنس الـ Domain لتغيير الحالة لـ Inactive (بدون مسح حقيقي)
        laptop.Deactivate(); // ميثود جوه الـ Laptop Entity عندك

        // 3. حفظ التغييرات
        await unitOfWork.SaveChangesAsync(ct);

        // ⚡ 4. تطهير الكاش فوراً عشان اللابتوب يختفي من المتجر في نفس اللحظة
        await cache.RemoveByTagAsync("laptops", ct);

        logger.LogInformation("Laptop {LaptopId} has been successfully deactivated.", command.LaptopId);

        return Result.Success;
    }
}