using LapBox.Application.Common.Interfaces.Caching;
using LapBox.Application.Common.Models;
using LapBox.Application.Features.Laptops.DTOs;
using LapBox.Domain.Common.Results;

namespace LapBox.Application.Features.Laptops.Queries.GetPagedLaptops;

public record GetPagedLaptopsQuery(
    string? SearchTerm,
    Guid? BrandId,
    int Page = 1,
    int PageSize = 10) : ICachedQuery<Result<PaginatedList<LaptopResponse>>>
{
    public string CacheKey => $"Laptops_page_{Page}_size_{PageSize}_search_{SearchTerm ?? "none"}_brand_{BrandId?.ToString() ?? "all"}";
    public string[] Tags => ["laptops"];
    public TimeSpan Expiration => TimeSpan.FromMinutes(10);
}