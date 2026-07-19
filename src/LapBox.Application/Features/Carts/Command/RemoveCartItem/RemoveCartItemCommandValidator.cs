using FluentValidation;

namespace LapBox.Application.Features.Carts.Command.RemoveCartItem;

public sealed class RemoveCartItemCommandValidator : AbstractValidator<RemoveCartItemCommand>
{
    public RemoveCartItemCommandValidator()
    {
        RuleFor(x => x.IdentityId)
            .NotEmpty().WithMessage("Identity identifier is required.");

        RuleFor(x => x.LaptopId)
            .NotEmpty().WithMessage("Laptop identifier is required.");
    }
}
