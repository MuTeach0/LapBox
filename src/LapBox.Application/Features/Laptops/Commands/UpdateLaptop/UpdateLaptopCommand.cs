using LapBox.Domain.Common.Results;
using MediatR;

namespace LapBox.Application.Features.Laptops.Commands.UpdateLaptop;

public record UpdateLaptopCommand(
    Guid LaptopId,
    Guid BrandId,
    Guid CategoryId,
    string Name,
    string Description,
    string Processor,
    string RAM,
    string Storage,
    string ScreenSize,
    string GraphicsCard) : IRequest<Result<Success>>;