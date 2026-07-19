using LapBox.Application.Features.Catalog.DTOs;
using LapBox.Domain.Common.Results;
using MediatR;

namespace LapBox.Application.Features.Catalog.Command.UpdateBrand;

public record UpdateBrandCommand(
    Guid Id,
    string Name,
    string Description,
    string? LogoUrl) : IRequest<Result<BrandResponse>>;