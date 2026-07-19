using LapBox.Application.Common.Interfaces.Caching;
using LapBox.Application.Features.Catalog.DTOs;
using LapBox.Domain.Common.Results;
using MediatR;

namespace LapBox.Application.Features.Catalog.Queries.GetAllCategories;

public record GetAllCategoriesQuery : ICachedQuery<Result<IReadOnlyList<CategoryResponse>>>
{
    public string CacheKey => "categories_all_active";

    public string[] Tags => ["categories"];

    public TimeSpan Expiration => TimeSpan.FromHours(2);
}
