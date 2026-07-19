using LapBox.Application.Common.Errors;
using LapBox.Application.Common.Interfaces.Persistence;
using LapBox.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace LapBox.Application.Features.Laptops.Commands.UpdateInventory;

public class UpdateLaptopInventoryCommandHandler(
    ILogger<UpdateLaptopInventoryCommandHandler> logger,
    ILaptopRepository laptopRepository,
    IUnitOfWork unitOfWork,
    HybridCache cache) : IRequestHandler<UpdateLaptopInventoryCommand, Result<Success>>
{
    public async Task<Result<Success>> Handle(UpdateLaptopInventoryCommand command, CancellationToken ct)
    {
        logger.LogInformation("Attempting to update inventory for Laptop ID: {LaptopId} by {QuantityChange}", 
            command.LaptopId, command.QuantityChange);

        // 1. جلب اللابتوب من المستودع
        var laptop = await laptopRepository.GetByIdAsync(command.LaptopId, ct);
        if (laptop is null)
        {
            logger.LogWarning("Inventory update failed. Laptop {LaptopId} not found.", command.LaptopId);
            return ApplicationErrors.LaptopNotFound;
        }

        // 2. استدعاء بزنس الـ Domain لفحص العملية (يمنع أن يصبح المخزن بالسالب)
        var updateResult = laptop.UpdateInventory(command.QuantityChange);
        if (updateResult.IsError)
        {
            logger.LogWarning("Inventory update denied by Domain for Laptop {LaptopId}. Reason: Insufficient Inventory.", 
                command.LaptopId);
            return updateResult.Errors;
        }

        // 3. حفظ التغييرات في قاعدة البيانات
        await unitOfWork.SaveChangesAsync(ct);

        // ⚡ 4. تطهير الكاش فوراً لتحديث كميات المخزن المتاحة بره في المتجر
        await cache.RemoveByTagAsync("laptops", ct);

        logger.LogInformation("Inventory for Laptop {LaptopId} successfully updated. New Quantity: {NewQuantity}", 
            command.LaptopId, laptop.InventoryQuantity);

        return Result.Success;
    }
}