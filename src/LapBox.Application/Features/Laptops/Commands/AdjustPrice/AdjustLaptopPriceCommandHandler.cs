using LapBox.Application.Common.Errors;
using LapBox.Application.Common.Interfaces.Persistence;
using LapBox.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace LapBox.Application.Features.Laptops.Commands.AdjustPrice;

public class AdjustLaptopPriceCommandHandler(
    ILogger<AdjustLaptopPriceCommandHandler> logger,
    ILaptopRepository laptopRepository,
    IUnitOfWork unitOfWork,
    HybridCache cache) : IRequestHandler<AdjustLaptopPriceCommand, Result<Success>>
{
    public async Task<Result<Success>> Handle(AdjustLaptopPriceCommand command, CancellationToken ct)
    {
        logger.LogInformation("Attempting to adjust price for Laptop ID: {LaptopId} to {NewPrice}", command.LaptopId, command.NewPrice);

        // 1. جلب اللابتوب من المستودع
        var laptop = await laptopRepository.GetByIdAsync(command.LaptopId, ct);
        if (laptop is null)
        {
            logger.LogWarning("Price adjustment failed. Laptop {LaptopId} not found.", command.LaptopId);
            return ApplicationErrors.LaptopNotFound; // تأكد من اسم الـ Error عندك
        }

        // 2. استدعاء البزنس ميثود من الـ Aggregate (واللي جواها بتعمل الـ Domain Event تلقائياً)
        var adjustResult = laptop.AdjustPrice(command.NewPrice);
        if (adjustResult.IsError)
        {
            logger.LogWarning("Price adjustment denied by Domain for Laptop {LaptopId}.", command.LaptopId);
            return adjustResult.Errors;
        }

        // 3. الحفظ (هنا الـ UnitOfWork هتعمل الـ Dispatch للـ Domain Event)
        await unitOfWork.SaveChangesAsync(ct);

        // ⚡ 4. تطهير الكاش فوراً عشان السعر الجديد يظهر في المتجر بره
        await cache.RemoveByTagAsync("laptops", ct);

        logger.LogInformation("Price for Laptop {LaptopId} successfully adjusted to {NewPrice}.", command.LaptopId, command.NewPrice);

        return Result.Success;
    }
}