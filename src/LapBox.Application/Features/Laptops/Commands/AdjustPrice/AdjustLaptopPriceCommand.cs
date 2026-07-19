using LapBox.Domain.Common.Results;
using MediatR;

namespace LapBox.Application.Features.Laptops.Commands.AdjustPrice;

public record AdjustLaptopPriceCommand(Guid LaptopId, decimal NewPrice) : IRequest<Result<Success>>;