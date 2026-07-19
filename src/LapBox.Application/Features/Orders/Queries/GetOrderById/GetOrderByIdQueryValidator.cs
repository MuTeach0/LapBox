using FluentValidation;

namespace LapBox.Application.Features.Orders.Queries.GetOrderById;

public class GetOrderByIdQueryValidator : AbstractValidator<GetOrderByIdQuery>
{
    public GetOrderByIdQueryValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("Order ID is required.")
            .NotEqual(Guid.Empty).WithMessage("Invalid Order ID format.");
    }
}