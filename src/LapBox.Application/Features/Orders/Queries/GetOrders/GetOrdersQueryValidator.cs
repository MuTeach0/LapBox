using FluentValidation;

namespace LapBox.Application.Features.Orders.Queries.GetOrders;

public class GetOrdersQueryValidator : AbstractValidator<GetOrdersQuery>
{
    public GetOrdersQueryValidator()
    {
        RuleFor(x => x.CurrentUserId).NotEmpty().WithMessage("Current User ID is required.");
        RuleFor(x => x.CurrentUserRole).NotEmpty().WithMessage("User role is required.");
    }
}