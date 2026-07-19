using FluentValidation;

namespace LapBox.Application.Features.Laptops.Commands.UpdateLaptop;

public sealed class UpdateLaptopCommandValidator : AbstractValidator<UpdateLaptopCommand>
{
    public UpdateLaptopCommandValidator()
    {
        RuleFor(x => x.LaptopId)
            .NotEmpty().WithMessage("Laptop ID is required.");

        RuleFor(x => x.BrandId)
            .NotEmpty().WithMessage("Brand ID is required.");

        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Category ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Laptop name is required.")
            .MaximumLength(150).WithMessage("Name cannot exceed 150 characters.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters.");

        // الفحص الأولي للمواصفات
        RuleFor(x => x.Processor)
            .NotEmpty().WithMessage("Processor specification is required.");

        RuleFor(x => x.RAM)
            .NotEmpty().WithMessage("RAM specification is required.");

        RuleFor(x => x.Storage)
            .NotEmpty().WithMessage("Storage specification is required.");

        RuleFor(x => x.ScreenSize)
            .NotEmpty().WithMessage("Screen size is required.");

        RuleFor(x => x.GraphicsCard)
            .NotEmpty().WithMessage("Graphics card specification is required.");
    }
}