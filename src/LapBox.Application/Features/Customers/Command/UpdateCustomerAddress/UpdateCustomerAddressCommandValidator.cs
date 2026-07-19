using FluentValidation;

namespace LapBox.Application.Features.Customers.Command.UpdateCustomerAddress;

public class UpdateCustomerAddressCommandValidator : AbstractValidator<UpdateCustomerAddressCommand>
{
    public UpdateCustomerAddressCommandValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty().WithMessage("Customer ID is required.");
        RuleFor(x => x.OldStreet).NotEmpty().WithMessage("Old street is required.");
        RuleFor(x => x.OldCity).NotEmpty().WithMessage("Old city is required.");
        
        RuleFor(x => x.NewStreet).NotEmpty().WithMessage("New street is required.");
        RuleFor(x => x.NewCity).NotEmpty().WithMessage("New city is required.");
        RuleFor(x => x.NewCountry).NotEmpty().WithMessage("New country is required.");
    }
}