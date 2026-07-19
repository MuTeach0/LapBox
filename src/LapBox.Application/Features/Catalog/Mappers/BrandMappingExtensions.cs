using LapBox.Application.Features.Catalog.DTOs;
using LapBox.Domain.Catalog;
using LapBox.Domain.Common.Results;

namespace LapBox.Application.Features.Catalog.Mappers;

public static class BrandMappingExtensions
{
    public static BrandResponse ToResponse(this Brand brand)
    {
        return new BrandResponse(
            brand.Id,
            brand.Name,
            brand.Description,
            brand.LogoUrl,
            brand.IsActive);
    }
    // public static Result<IReadOnlyList<BrandResponse>> ToResponseList(this IEnumerable<Brand> brands) => 
    //     brands.Select(b => b.ToResponse()).ToList().AsReadOnly();
}