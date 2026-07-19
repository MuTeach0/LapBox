namespace LapBox.Application.Features.Catalog.DTOs;

public record CategoryResponse(
    Guid Id,
    string Name,
    string Description,
    bool IsActive);
