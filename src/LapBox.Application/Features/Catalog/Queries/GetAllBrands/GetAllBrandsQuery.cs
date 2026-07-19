using LapBox.Application.Common.Interfaces.Caching;
using LapBox.Application.Features.Catalog.DTOs;
using LapBox.Domain.Common.Results;
using MediatR;

namespace LapBox.Application.Features.Catalog.Queries.GetAllBrands;
public record GetAllBrandsQuery : ICachedQuery<Result<IReadOnlyList<BrandResponse>>>
{
    public string CacheKey => "brands_all_active";

    // 🎯 الـ Tag الموحد اللي الـ Event Handler هيهده
    public string[] Tags => ["brands"];

    // ⏱️ صلاحية الكاش (ساعتين مثلاً)
    public TimeSpan Expiration => TimeSpan.FromHours(2);
}