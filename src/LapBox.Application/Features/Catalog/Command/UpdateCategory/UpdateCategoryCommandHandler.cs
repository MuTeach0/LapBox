using LapBox.Application.Common.Interfaces.Persistence;
using LapBox.Application.Features.Catalog.DTOs;
using LapBox.Application.Features.Catalog.Mappers;
using LapBox.Domain.Catalog;
using LapBox.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LapBox.Application.Features.Catalog.Command.UpdateCategory;

public class UpdateCategoryCommandHandler(
    ICategoryRepository categoryRepository,
    IUnitOfWork unitOfWork,
    ILogger<UpdateCategoryCommandHandler> logger) : IRequestHandler<UpdateCategoryCommand, Result<CategoryResponse>>
{
    public async Task<Result<CategoryResponse>> Handle(UpdateCategoryCommand command, CancellationToken ct)
    {
        logger.LogInformation("Updating category ID: {CategoryId}", command.Id);

        var category = await categoryRepository.GetByIdAsync(command.Id, ct);
        if (category is null)
        {
            return CategoryErrors.CategoryNotFound;
        }

        // Check if name is being changed and if the new name already exists
        if (!category.Name.Equals(command.Name, StringComparison.OrdinalIgnoreCase))
        {
            bool nameExists = await categoryRepository.ExistsByNameAsync(command.Name, ct);
            if (nameExists)
            {
                return Error.Conflict("Category.NameExists", $"A category with the name '{command.Name}' already exists.");
            }
        }

        var updateResult = category.Update(command.Name, command.Description);
        if (updateResult.IsError)
        {
            return updateResult.Errors;
        }

        await unitOfWork.SaveChangesAsync(ct);
        return category.ToResponse();
    }
}
