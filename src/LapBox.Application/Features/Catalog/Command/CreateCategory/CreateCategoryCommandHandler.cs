using LapBox.Application.Common.Errors;
using LapBox.Application.Common.Interfaces.Persistence;
using LapBox.Application.Features.Catalog.DTOs;
using LapBox.Domain.Catalog;
using LapBox.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LapBox.Application.Features.Catalog.Command.CreateCategory;

public sealed class CreateCategoryCommandHandler(
    ICategoryRepository categoryRepository,
    ILogger<CreateCategoryCommandHandler> logger,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateCategoryCommand, Result<CategoryResponse>>
{
    public async Task<Result<CategoryResponse>> Handle(CreateCategoryCommand command, CancellationToken ct)
    {
        logger.LogInformation("Attempting to create a new category with name: {CategoryName}", command.Name);

        bool categoryExists = await categoryRepository.ExistsByNameAsync(command.Name, ct);
        if (categoryExists)
        {
            logger.LogWarning("Category creation failed. Name '{CategoryName}' already exists.", command.Name);
            return ApplicationErrors.CategoryNameAlreadyExists(command.Name); 
        }

        var categoryResult = Category.Create(command.Name, command.Description);
        if (categoryResult.IsError) return categoryResult.Errors;

        await categoryRepository.AddAsync(categoryResult.Value, ct);
        await unitOfWork.SaveChangesAsync(ct);

        logger.LogInformation("Evicting 'categories' cache tag due to new category creation.");

        return new CategoryResponse(
            categoryResult.Value.Id,
            categoryResult.Value.Name,
            categoryResult.Value.Description,
            categoryResult.Value.IsActive);
    }
}
