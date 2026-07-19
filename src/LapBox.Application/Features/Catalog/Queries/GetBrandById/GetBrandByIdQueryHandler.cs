using LapBox.Application.Common.Errors;
using LapBox.Application.Common.Interfaces.Persistence;
using LapBox.Application.Features.Catalog.DTOs;
using LapBox.Application.Features.Catalog.Mappers;
using LapBox.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LapBox.Application.Features.Catalog.Queries.GetBrandById;

public class GetBrandByIdQueryHandler(
    IBrandRepository brandRepository,
    ILogger<GetBrandByIdQueryHandler> logger) : IRequestHandler<GetBrandByIdQuery, Result<BrandResponse>>
{
    public async Task<Result<BrandResponse>> Handle(GetBrandByIdQuery query, CancellationToken ct)
    {
        logger.LogInformation("Executing GetBrandByIdQuery for ID: {BrandId}", query.Id);
        var brand = await brandRepository.GetByIdAsync(query.Id, ct);

        // لو الكاش رجع نل (معناه مش موجود في الداتا بيز اصلاً) بنرجع الـ NotFound Error الموحد
        if (brand is null)
        {
            logger.LogWarning("Brand with ID {BrandId} was not found.", query.Id);
            return ApplicationErrors.BrandNotFound;
        }

        return brand.ToResponse();
    }
}