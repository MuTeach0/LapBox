using FluentValidation;

namespace LapBox.Application.Features.Laptops.Commands.AdjustPrice;

public sealed class AdjustLaptopPriceCommandValidator : AbstractValidator<AdjustLaptopPriceCommand>
{
    public AdjustLaptopPriceCommandValidator()
    {
        RuleFor(x => x.LaptopId)
            .NotEmpty().WithMessage("Laptop ID is required.");

        RuleFor(x => x.NewPrice)
            .GreaterThan(0).WithMessage("The new price must be greater than 0.");
    }
}