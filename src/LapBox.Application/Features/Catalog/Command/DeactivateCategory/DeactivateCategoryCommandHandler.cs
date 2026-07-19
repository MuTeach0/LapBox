using LapBox.Application.Common.Errors;
using LapBox.Application.Common.Interfaces.Persistence;
using LapBox.Domain.Catalog;
using LapBox.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace LapBox.Application.Features.Catalog.Command.DeactivateCategory;

public class DeactivateCategoryCommandHandler(
    ICategoryRepository categoryRepository,
    IUnitOfWork unitOfWork,
    HybridCache cache,
    ILaptopRepository laptopRepository,
    ILogger<DeactivateCategoryCommandHandler> logger) : IRequestHandler<DeactivateCategoryCommand, Result<Success>>
{
    public async Task<Result<Success>> Handle(DeactivateCategoryCommand command, CancellationToken ct)
    {
        logger.LogInformation("Deactivating category ID: {CategoryId}", command.Id);

        var category = await categoryRepository.GetByIdAsync(command.Id, ct);
        if (category is null)
        {
            return ApplicationErrors.CategoryNotFound;
        }

        bool hasActiveLaptops = await laptopRepository.HasActiveLaptopsWithBrandIdAsync(command.Id, ct);
        if (hasActiveLaptops)
        {
            return ApplicationErrors.CategoryHasActiveLaptops;
        }

        var deactivateResult = category.Deactivate();
        if (deactivateResult.IsError)
        {
            return deactivateResult.Errors;
        }

        await unitOfWork.SaveChangesAsync(ct);

        await cache.RemoveByTagAsync("categories", ct); // Evict the cache for categories

        return Result.Success;
    }
}
