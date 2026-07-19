using LapBox.Application.Common.Errors;
using LapBox.Application.Common.Interfaces.Persistence;
using LapBox.Domain.Common.Results;
using LapBox.Domain.Laptops.ValueObjects;
using MediatR;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace LapBox.Application.Features.Laptops.Commands.UpdateLaptop;

public class UpdateLaptopCommandHandler(
    ILogger<UpdateLaptopCommandHandler> logger,
    ILaptopRepository laptopRepository,
    IUnitOfWork unitOfWork,
    HybridCache cache) : IRequestHandler<UpdateLaptopCommand, Result<Success>>
{
    public async Task<Result<Success>> Handle(UpdateLaptopCommand command, CancellationToken ct)
    {
        logger.LogInformation("Attempting to update details for Laptop ID: {LaptopId}", command.LaptopId);

        // 1. جلب اللابتوب الحالي من الداتا بيز
        var laptop = await laptopRepository.GetByIdAsync(command.LaptopId, ct);
        if (laptop is null)
        {
            logger.LogWarning("Update failed. Laptop {LaptopId} not found.", command.LaptopId);
            return ApplicationErrors.LaptopNotFound; 
        }

        // 2. 🧠 بناء وفحص الـ Specification (Value Object) أولاً كـ خط دفاع للـ Domain
        var specResult = Specification.Create(
            command.Processor,
            command.RAM,
            command.Storage,
            command.ScreenSize,
            command.GraphicsCard
        );

        if (specResult.IsError)
        {
            logger.LogWarning("Specification validation failed during Laptop {LaptopId} update.", command.LaptopId);
            return specResult.Errors;
        }

        // 3. 🚀 استدعاء ميثود الـ UpdateDetails من الـ Aggregate Root لتحديث البيانات
        var updateResult = laptop.UpdateDetails(
            command.BrandId,
            command.CategoryId,
            command.Name,
            command.Description,
            specResult.Value
        );

        if (updateResult.IsError)
        {
            logger.LogWarning("Domain validation failed for updating Laptop {LaptopId}.", command.LaptopId);
            return updateResult.Errors;
        }

        // 4. حفظ التغييرات في قاعدة البيانات
        await unitOfWork.SaveChangesAsync(ct);

        // ⚡ 5. تطهير الكاش فوراً عشان التحديث يظهر بره للعملاء في نفس اللحظة
        await cache.RemoveByTagAsync("laptops", ct);

        logger.LogInformation("Laptop {LaptopId} details successfully updated.", command.LaptopId);

        return Result.Success;
    }
}