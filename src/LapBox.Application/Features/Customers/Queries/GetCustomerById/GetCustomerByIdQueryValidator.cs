using FluentValidation;

namespace LapBox.Application.Features.Customers.Queries.GetCustomerById;

public class GetCustomerByIdQueryValidator : AbstractValidator<GetCustomerByIdQuery>
{
    public GetCustomerByIdQueryValidator()
    {
        RuleFor(x => x.Id).NotEmpty()
            .NotEmpty()
            .WithErrorCode("CustomerId_Is_Required")
            .WithMessage("CustomerId is required.");;
    }
}