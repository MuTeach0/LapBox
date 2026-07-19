using LapBox.Application.Common.Interfaces.Caching;
using LapBox.Application.Features.Catalog.DTOs;
using LapBox.Domain.Common.Results;
using MediatR;

namespace LapBox.Application.Features.Catalog.Queries.GetBrandById;

public record GetBrandByIdQuery(Guid Id) : ICachedQuery<Result<BrandResponse>>
{
    // ⚡ كاش كي ديناميكي لكل براند
    public string CacheKey => $"brand_{Id}";

    // 🎯 مربوط بنفس الـ Tag عشان لو حصل Update للبراند ده بالذات، الكاش بتاعه يطير
    public string[] Tags => ["brands"];

    public TimeSpan Expiration => TimeSpan.FromHours(4);
}