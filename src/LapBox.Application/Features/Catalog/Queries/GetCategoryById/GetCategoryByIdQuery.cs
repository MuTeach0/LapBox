using LapBox.Application.Common.Interfaces.Caching;
using LapBox.Application.Features.Catalog.DTOs;
using LapBox.Domain.Common.Results;
using MediatR;

namespace LapBox.Application.Features.Catalog.Queries.GetCategoryById;

public record GetCategoryByIdQuery(Guid Id) : ICachedQuery<Result<CategoryResponse>>
{
    public string CacheKey => $"category_{Id}";

    public string[] Tags => ["categories"];

    public TimeSpan Expiration => TimeSpan.FromHours(4);
}
