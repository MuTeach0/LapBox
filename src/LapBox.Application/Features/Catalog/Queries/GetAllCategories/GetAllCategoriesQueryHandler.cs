using LapBox.Application.Common.Interfaces.Persistence;
using LapBox.Application.Features.Catalog.DTOs;
using LapBox.Application.Features.Catalog.Mappers;
using LapBox.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LapBox.Application.Features.Catalog.Queries.GetAllCategories;

public class GetAllCategoriesQueryHandler(
    ICategoryRepository categoryRepository,
    ILogger<GetAllCategoriesQueryHandler> logger) : IRequestHandler<GetAllCategoriesQuery, Result<IReadOnlyList<CategoryResponse>>>
{
    public async Task<Result<IReadOnlyList<CategoryResponse>>> Handle(GetAllCategoriesQuery query, CancellationToken ct)
    {
        logger.LogInformation("Fetching active categories directly from Database (Pipeline Cache Miss)...");

        var categories = await categoryRepository.GetActiveCategoriesAsync(ct);
        var list = categories.Select(c => c.ToResponse()).ToList().AsReadOnly();
        return list;
    }
}
