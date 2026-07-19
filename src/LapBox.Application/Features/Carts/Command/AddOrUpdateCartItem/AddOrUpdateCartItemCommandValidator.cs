using FluentValidation;

namespace LapBox.Application.Features.Carts.Command.AddOrUpdateCartItem;

public sealed class AddOrUpdateCartItemCommandValidator : AbstractValidator<AddOrUpdateCartItemCommand>
{
    public AddOrUpdateCartItemCommandValidator()
    {
        RuleFor(x => x.IdentityId)
            .NotEmpty().WithMessage("Identity identifier is required.");

        RuleFor(x => x.LaptopId)
            .NotEmpty().WithMessage("Laptop identifier is required.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than zero.");

        RuleFor(x => x.UnitPrice)
            .GreaterThan(0).WithMessage("Unit price must be greater than zero.");
    }
}
