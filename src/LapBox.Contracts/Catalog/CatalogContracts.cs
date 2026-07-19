namespace LapBox.Contracts.Catalog;

public sealed record BrandResponse(
    Guid Id,
    string Name,
    string Description,
    string? LogoUrl,
    bool IsActive);

public sealed record CategoryResponse(
    Guid Id,
    string Name,
    string Description,
    bool IsActive);

public sealed record CreateBrandRequest(
    string Name,
    string Description,
    string? LogoUrl);

public sealed record UpdateBrandRequest(
    string Name,
    string Description,
    string? LogoUrl);

public sealed record CreateCategoryRequest(
    string Name,
    string Description);

public sealed record UpdateCategoryRequest(
    string Name,
    string? Description);
