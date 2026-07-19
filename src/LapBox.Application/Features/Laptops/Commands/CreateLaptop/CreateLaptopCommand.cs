using LapBox.Application.Features.Laptops.DTOs;
using LapBox.Domain.Common.Results;
using MediatR;

namespace LapBox.Application.Features.Laptops.Commands.CreateLaptop;

public record CreateLaptopCommand(
    Guid BrandId,
    Guid CategoryId,
    string Name,
    string Sku,
    string Description,
    decimal BasePrice,
    int InventoryQuantity,
    string Processor,
    string RAM,
    string Storage,
    string ScreenSize,
    string GraphicsCard) : IRequest<Result<LaptopResponse>>;
