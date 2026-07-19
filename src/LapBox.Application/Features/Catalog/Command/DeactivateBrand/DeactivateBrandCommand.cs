using LapBox.Domain.Common.Results;
using MediatR;

namespace LapBox.Application.Features.Catalog.Command.DeactivateBrand;

public record DeactivateBrandCommand(Guid Id) : IRequest<Result<Success>>;