using LapBox.Application.Common.Errors;
using LapBox.Application.Common.Interfaces.Persistence;
using LapBox.Application.Features.Catalog.DTOs;
using LapBox.Application.Features.Catalog.Events;
using LapBox.Application.Features.Catalog.Mappers;
using LapBox.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LapBox.Application.Features.Catalog.Command.UpdateBrand;

public class UpdateBrandCommandHandler(
    IBrandRepository brandRepository,
    IUnitOfWork unitOfWork,
    ILogger<UpdateBrandCommandHandler> logger) : IRequestHandler<UpdateBrandCommand, Result<BrandResponse>>
{
    public async Task<Result<BrandResponse>> Handle(UpdateBrandCommand command, CancellationToken ct)
    {
        logger.LogInformation("Updating brand ID: {BrandId}", command.Id);

        // 1. جلب البراند من الداتا بيز
        var brand = await brandRepository.GetByIdAsync(command.Id, ct);
        if (brand is null)
        {
            return ApplicationErrors.BrandNotFound;
        }

        // 2. حماية البزنس: لو الاسم اتغير، نتأكد إنه مش متكرر مع براند تاني
        if (!brand.Name.Equals(command.Name, StringComparison.OrdinalIgnoreCase))
        {
            bool nameExists = await brandRepository.ExistsByNameAsync(command.Name, ct);
            if (nameExists)
            {
                return ApplicationErrors.BrandNameAlreadyExists(command.Name);
            }
        }

        // 3. تحديث الدومين
        var updateResult = brand.Update(command.Name, command.Description, command.LogoUrl);
        if (updateResult.IsError)
        {
            return updateResult.Errors;
        }

        await unitOfWork.SaveChangesAsync(ct);
        return brand.ToResponse();
    }
}