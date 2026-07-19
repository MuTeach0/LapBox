using FluentValidation;

namespace LapBox.Application.Features.Customers.Command.CreateCustomer;

public sealed class CreateCustomerCommandValidator : AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerCommandValidator()
    {
        RuleFor(x => x.IdentityId)
            .NotEmpty().WithMessage("IdentityId is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Customer name is required.")
            .MaximumLength(100).WithMessage("Customer name must not exceed 100 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email address is required.")
            .EmailAddress().WithMessage("A valid email address format is required.")
            .MaximumLength(255).WithMessage("Email must not exceed 255 characters.");

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20).WithMessage("Phone number must not exceed 20 characters.")
            .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("Invalid international phone number format.")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber));
    }
}