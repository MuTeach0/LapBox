using LapBox.Application.Common.Errors;
using LapBox.Application.Common.Interfaces.Persistence;
using LapBox.Application.Features.Catalog.DTOs;
using LapBox.Application.Features.Catalog.Mappers;
using LapBox.Domain.Catalog;
using LapBox.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LapBox.Application.Features.Catalog.Command.CreateBrand;

public class CreateBrandCommandHandler(
    IBrandRepository brandRepository,
    IUnitOfWork unitOfWork, // بفرض إن عندك UnitOfWork لتسجيل التغييرات
    ILogger<CreateBrandCommandHandler> logger) : IRequestHandler<CreateBrandCommand, Result<BrandResponse>>
{
    public async Task<Result<BrandResponse>> Handle(CreateBrandCommand command, CancellationToken ct)
    {
        logger.LogInformation("Attempting to create a new brand with name: {BrandName}", command.Name);

        // 1. تحقق من عدم تكرار الاسم في الداتا بيز حماية للبزنس
        bool brandExists = await brandRepository.ExistsByNameAsync(command.Name, ct);
        if (brandExists)
        {
            logger.LogWarning("Brand creation failed. Name '{BrandName}' already exists.", command.Name);
            return ApplicationErrors.BrandNameAlreadyExists(command.Name);
        }

        // 2. إنشاء الـ Aggregate Root من خلال الـ Factory Method
        var brandResult = Brand.Create(command.Name, command.Description, command.LogoUrl);
        if (brandResult.IsError)
        {
            logger.LogWarning("Brand creation failed due to domain validation errors.");
            return brandResult.Errors;
        }

        // 3. الحفظ في قاعدة البيانات
        await brandRepository.AddAsync(brandResult.Value, ct);
        await unitOfWork.SaveChangesAsync(ct);

        // ⚡ 4. تطهير كاش البراندات فوراً عشان أي كاش قديم يتهد والجديد يظهر
        logger.LogInformation("Evicting 'brands' cache tag due to new brand creation.");

        logger.LogInformation("Brand '{BrandName}' created successfully with ID: {BrandId}", command.Name, brandResult.Value.Id);

        return brandResult.Value.ToResponse();
    }
}