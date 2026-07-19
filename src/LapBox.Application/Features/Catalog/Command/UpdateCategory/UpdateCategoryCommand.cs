using LapBox.Application.Features.Catalog.DTOs;
using LapBox.Domain.Common.Results;
using MediatR;

namespace LapBox.Application.Features.Catalog.Command.UpdateCategory;

public record UpdateCategoryCommand(
    Guid Id,
    string Name,
    string Description) : IRequest<Result<CategoryResponse>>;
