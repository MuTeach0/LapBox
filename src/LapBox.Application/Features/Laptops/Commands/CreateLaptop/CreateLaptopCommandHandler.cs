using LapBox.Application.Common.Errors;
using LapBox.Application.Common.Interfaces.Persistence;
using LapBox.Application.Features.Laptops.DTOs;
using LapBox.Application.Features.Laptops.Mappers;
using LapBox.Domain.Common.Results;
using LapBox.Domain.Laptops;
using LapBox.Domain.Laptops.ValueObjects;
using MediatR;

namespace LapBox.Application.Features.Laptops.Commands.CreateLaptop;

public sealed class CreateLaptopCommandHandler(
    ILaptopRepository laptopRepository,
    IBrandRepository brandRepository,
    ICategoryRepository categoryRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateLaptopCommand, Result<LaptopResponse>>
{
    public async Task<Result<LaptopResponse>> Handle(CreateLaptopCommand command, CancellationToken ct)
    {
        // 1. Validate brand and category exist
        var brand = await brandRepository.GetByIdAsync(command.BrandId, ct);
        if (brand is null)
            return ApplicationErrors.BrandNotFound;
            // return Error.NotFound("Brand.NotFound", $"Brand with ID {command.BrandId} was not found.");

        var category = await categoryRepository.GetByIdAsync(command.CategoryId, ct);
        if (category is null)
            return ApplicationErrors.CategoryNotFound;
            // return Error.NotFound("Category.NotFound", $"Category with ID {command.CategoryId} was not found.");

        bool isSkuDuplicate = await laptopRepository.ExistsBySkuAsync(command.Sku, ct);
        if (isSkuDuplicate)
            return ApplicationErrors.LaptopAlreadyExists(command.Sku);

        // 2. Build the Specification VO
        var specResult = Specification.Create(
            command.Processor,
            command.RAM,
            command.Storage,
            command.ScreenSize,
            command.GraphicsCard);
        if (specResult.IsError) return specResult.Errors;

        // 3. Build the Laptop aggregate
        var laptopResult = Laptop.Create(
            command.BrandId,
            command.CategoryId,
            command.Name,
            command.Sku,
            command.Description,
            command.BasePrice,
            command.InventoryQuantity,
            specResult.Value);
        if (laptopResult.IsError) return laptopResult.Errors;

        // 4. Persist
        await laptopRepository.AddAsync(laptopResult.Value, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return laptopResult.Value.ToResponse();
    }
}
