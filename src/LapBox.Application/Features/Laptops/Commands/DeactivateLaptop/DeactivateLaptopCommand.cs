using LapBox.Domain.Common.Results;
using MediatR;

namespace LapBox.Application.Features.Laptops.Commands.DeactivateLaptop;

public record DeactivateLaptopCommand(Guid LaptopId) : IRequest<Result<Success>>;