namespace LapBox.Application.Features.Catalog.DTOs;

public record BrandResponse(
    Guid Id,
    string Name,
    string Description,
    string? LogoUrl,
    bool IsActive);