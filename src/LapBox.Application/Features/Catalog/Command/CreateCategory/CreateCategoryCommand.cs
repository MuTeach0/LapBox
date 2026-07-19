using LapBox.Application.Features.Catalog.DTOs;
using LapBox.Domain.Common.Results;
using MediatR;

namespace LapBox.Application.Features.Catalog.Command.CreateCategory;

public record CreateCategoryCommand(
    string Name,
    string Description) : IRequest<Result<CategoryResponse>>;
