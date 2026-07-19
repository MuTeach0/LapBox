using FluentValidation;

namespace LapBox.Application.Features.Carts.Command.ClearCart;

public sealed class ClearCartCommandValidator : AbstractValidator<ClearCartCommand>
{
    public ClearCartCommandValidator()
    {
        RuleFor(x => x.IdentityId)
            .NotEmpty().WithMessage("Identity identifier is required.");
    }
}
