using LapBox.Application.Common.Errors;
using LapBox.Application.Common.Interfaces.Persistence;
using LapBox.Application.Features.Laptops.DTOs;
using LapBox.Application.Features.Laptops.Mappers;
using LapBox.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LapBox.Application.Features.Laptops.Queries.GetLaptopById;

public class GetLaptopByIdQueryHandler(
    ILogger<GetLaptopByIdQueryHandler> logger,
    ILaptopRepository laptopRepository) : IRequestHandler<GetLaptopByIdQuery, Result<LaptopResponse>>
{
    public async Task<Result<LaptopResponse>> Handle(GetLaptopByIdQuery query, CancellationToken ct)
    {
        logger.LogInformation("Fetching details for Laptop ID: {LaptopId}", query.LaptopId);

        var laptop = await laptopRepository.GetByIdAsync(query.LaptopId, ct);
        if (laptop is null)
        {
            logger.LogWarning("Laptop ID: {LaptopId} was not found in Database.", query.LaptopId);
            return ApplicationErrors.LaptopNotFound;
        }
        return laptop.ToResponse();
    }
}