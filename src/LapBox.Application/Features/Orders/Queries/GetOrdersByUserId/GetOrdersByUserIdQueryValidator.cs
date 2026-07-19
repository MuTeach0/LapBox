using FluentValidation;

namespace LapBox.Application.Features.Orders.Queries.GetOrdersByUserId;

public class GetOrdersByUserIdQueryValidator : AbstractValidator<GetOrdersByUserIdQuery>
{
    public GetOrdersByUserIdQueryValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("Target User ID is required.");
        RuleFor(x => x.CurrentUserId).NotEmpty().WithMessage("Current User ID is required.");
        RuleFor(x => x.CurrentUserRole).NotEmpty().WithMessage("User role is required.");
        RuleFor(x => x.UserId).NotEmpty().WithMessage("User ID is required.");
    }
}