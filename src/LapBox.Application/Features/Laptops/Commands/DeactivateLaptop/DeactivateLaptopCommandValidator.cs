using FluentValidation;

namespace LapBox.Application.Features.Laptops.Commands.DeactivateLaptop;

public sealed class DeactivateLaptopCommandValidator : AbstractValidator<DeactivateLaptopCommand>
{
    public DeactivateLaptopCommandValidator()
    {
        RuleFor(x => x.LaptopId)
            .NotEmpty().WithMessage("Laptop ID is required to perform deactivation.");
    }
}