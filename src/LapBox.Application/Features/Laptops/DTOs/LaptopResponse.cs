namespace LapBox.Application.Features.Laptops.DTOs;

public record LaptopResponse(
    Guid Id,
    Guid BrandId,
    string Name,
    string Description,
    decimal BasePrice,
    int InventoryQuantity,
    SpecificationResponse Specification);