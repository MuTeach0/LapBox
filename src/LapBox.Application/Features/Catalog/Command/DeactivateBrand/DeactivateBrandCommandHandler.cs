using LapBox.Application.Common.Errors;
using LapBox.Application.Common.Interfaces.Persistence;
using LapBox.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace LapBox.Application.Features.Catalog.Command.DeactivateBrand;

public class DeactivateBrandCommandHandler(
    IBrandRepository brandRepository,
    ILaptopRepository laptopRepository, // 👈 حقن مستودع اللابتوبات لحماية العلاقات
    IUnitOfWork unitOfWork,
    ILogger<DeactivateBrandCommandHandler> logger,
    HybridCache cache) : IRequestHandler<DeactivateBrandCommand, Result<Success>>
{
    public async Task<Result<Success>> Handle(DeactivateBrandCommand command, CancellationToken ct)
    {
        logger.LogInformation("Deactivating brand ID: {BrandId}", command.Id);

        var brand = await brandRepository.GetByIdAsync(command.Id, ct);
        if (brand is null)
        {
            return ApplicationErrors.BrandNotFound;
        }

        // 🛡️ تشيك الأمان: نمنع الإيقاف لو فيه لابتوبات معتمدة عليه حالياً
        // (افترضنا إن الميثود دي موجودة أو هتلحقها في الـ LaptopRepository)
        bool hasActiveLaptops = await laptopRepository.HasActiveLaptopsWithBrandIdAsync(command.Id, ct);
        if (hasActiveLaptops)
        {
            return ApplicationErrors.BrandHasActiveLaptops;
        }

        var deactivateResult = brand.Deactivate();
        if (deactivateResult.IsError)
        {
            return deactivateResult.Errors;
        }

        await unitOfWork.SaveChangesAsync(ct);
        await cache.RemoveByTagAsync("brands", ct);


        return Result.Success;
    }
}