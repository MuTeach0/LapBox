using LapBox.Application.Features.Catalog.DTOs;
using LapBox.Domain.Common.Results;
using MediatR;

namespace LapBox.Application.Features.Catalog.Command.CreateBrand;

public record CreateBrandCommand(
    string Name,
    string Description,
    string? LogoUrl) : IRequest<Result<BrandResponse>>;