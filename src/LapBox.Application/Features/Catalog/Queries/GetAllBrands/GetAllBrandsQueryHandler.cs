using LapBox.Application.Common.Interfaces.Persistence;
using LapBox.Application.Features.Catalog.DTOs;
using LapBox.Application.Features.Catalog.Mappers;
using LapBox.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LapBox.Application.Features.Catalog.Queries.GetAllBrands;

public class GetAllBrandsQueryHandler(
    IBrandRepository brandRepository,
    ILogger<GetAllBrandsQueryHandler> logger) : IRequestHandler<GetAllBrandsQuery, Result<IReadOnlyList<BrandResponse>>>
{
    public async Task<Result<IReadOnlyList<BrandResponse>>> Handle(GetAllBrandsQuery query, CancellationToken ct)
    {
        logger.LogInformation("Fetching active brands directly from Database (Pipeline Cache Miss)...");

        var brands = await brandRepository.GetActiveBrandsAsync(ct);
        var list = brands.Select(b => b.ToResponse()).ToList().AsReadOnly();
        return list;
    }
}