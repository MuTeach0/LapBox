using LapBox.Application.Features.Catalog.DTOs;
using LapBox.Domain.Catalog;

namespace LapBox.Application.Features.Catalog.Mappers;

public static class CategoryMappingExtensions
{
    public static CategoryResponse ToResponse(this Category category)
    {
        return new CategoryResponse(
            category.Id,
            category.Name,
            category.Description,
            category.IsActive);
    }
}
