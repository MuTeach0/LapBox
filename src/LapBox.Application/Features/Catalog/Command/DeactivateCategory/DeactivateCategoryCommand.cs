using LapBox.Domain.Common.Results;
using MediatR;

namespace LapBox.Application.Features.Catalog.Command.DeactivateCategory;

public record DeactivateCategoryCommand(Guid Id) : IRequest<Result<Success>>;
