using FluentValidation;

namespace LapBox.Application.Features.Orders.Command.CreateOrder;

public sealed class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        // IdentityId comes from authentication context, no need to validate
        RuleFor(x => x.ShippingStreet).NotEmpty().WithMessage("Street is required.");
        RuleFor(x => x.ShippingCity).NotEmpty().WithMessage("City is required.");
        RuleFor(x => x.ShippingCountry).NotEmpty().WithMessage("Country is required.");
        // Reservations are validated in the handler - they must exist and be active
    }
}