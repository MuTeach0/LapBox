using LapBox.Application.Common.Interfaces.Persistence;
using LapBox.Application.Features.Catalog.DTOs;
using LapBox.Application.Features.Catalog.Mappers;
using LapBox.Domain.Catalog;
using LapBox.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LapBox.Application.Features.Catalog.Queries.GetCategoryById;

public class GetCategoryByIdQueryHandler(
    ICategoryRepository categoryRepository,
    ILogger<GetCategoryByIdQueryHandler> logger) : IRequestHandler<GetCategoryByIdQuery, Result<CategoryResponse>>
{
    public async Task<Result<CategoryResponse>> Handle(GetCategoryByIdQuery query, CancellationToken ct)
    {
        logger.LogInformation("Executing GetCategoryByIdQuery for ID: {CategoryId}", query.Id);
        var category = await categoryRepository.GetByIdAsync(query.Id, ct);

        if (category is null)
        {
            logger.LogWarning("Category with ID {CategoryId} was not found.", query.Id);
            return CategoryErrors.CategoryNotFound;
        }

        return category.ToResponse();
    }
}
