using FluentValidation;

namespace LapBox.Application.Features.Customers.Queries.GetCustomerByIdentityId;

public class GetCustomerByIdentityIdQueryValidator : AbstractValidator<GetCustomerByIdentityIdQuery>
{
    public GetCustomerByIdentityIdQueryValidator()
    {
        RuleFor(x => x.IdentityId).NotEmpty();
    }
}
