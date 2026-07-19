using FluentValidation;

namespace LapBox.Application.Features.Laptops.Commands.CreateLaptop;

public sealed class CreateLaptopCommandValidator : AbstractValidator<CreateLaptopCommand>
{
    public CreateLaptopCommandValidator()
    {
        RuleFor(x => x.BrandId)
            .NotEmpty().WithMessage("Brand ID is required.");

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Category ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Laptop name is required.")
            .MaximumLength(200).WithMessage("Laptop name cannot exceed 200 characters.");

        RuleFor(x => x.Sku)
            .NotEmpty().WithMessage("SKU is required.")
            .MaximumLength(100).WithMessage("SKU cannot exceed 100 characters.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters.");

        RuleFor(x => x.BasePrice)
            .GreaterThan(0).WithMessage("Base price must be greater than zero.");

        RuleFor(x => x.InventoryQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Inventory quantity cannot be negative.");

        RuleFor(x => x.Processor)
            .NotEmpty().WithMessage("Processor is required.");

        RuleFor(x => x.RAM)
            .NotEmpty().WithMessage("RAM is required.");

        RuleFor(x => x.Storage)
            .NotEmpty().WithMessage("Storage is required.");

        RuleFor(x => x.ScreenSize)
            .NotEmpty().WithMessage("Screen size is required.");

        RuleFor(x => x.GraphicsCard)
            .NotEmpty().WithMessage("Graphics card is required.");
    }
}
