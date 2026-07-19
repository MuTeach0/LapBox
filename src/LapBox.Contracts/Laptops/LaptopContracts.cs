namespace LapBox.Contracts.Laptops;

public sealed record SpecificationResponse(
    string Processor,
    string RAM,
    string Storage,
    string ScreenSize,
    string GraphicsCard);

public sealed record LaptopResponse(
    Guid Id,
    Guid BrandId,
    string Name,
    string Description,
    decimal BasePrice,
    int InventoryQuantity,
    SpecificationResponse Specification,
    IReadOnlyList<LaptopImageResponse>? Images);

public sealed record LaptopImageResponse(
    string Url,
    bool IsMain,
    int DisplayOrder);

public sealed record CreateLaptopRequest(
    Guid BrandId,
    Guid CategoryId,
    string Name,
    string Sku,
    string Description,
    decimal BasePrice,
    int InventoryQuantity,
    string Processor,
    string RAM,
    string Storage,
    string ScreenSize,
    string GraphicsCard);

public sealed record UpdateLaptopRequest(
    Guid BrandId,
    Guid CategoryId,
    string Name,
    string Description,
    string Processor,
    string RAM,
    string Storage,
    string ScreenSize,
    string GraphicsCard);

public sealed record UpdateLaptopInventoryRequest(int Quantity);

public sealed record AdjustLaptopPriceRequest(decimal NewPrice);

public sealed record GetPagedLaptopsRequest(
    string? SearchTerm = null,
    Guid? BrandId = null,
    int Page = 1,
    int PageSize = 10);
