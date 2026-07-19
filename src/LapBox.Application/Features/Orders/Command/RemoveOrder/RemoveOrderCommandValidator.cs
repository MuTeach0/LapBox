using FluentValidation;

namespace LapBox.Application.Features.Orders.Command.RemoveOrder;

public sealed class RemoveOrderCommandValidator : AbstractValidator<RemoveOrderCommand>
{
    public RemoveOrderCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty().WithMessage("Order ID is required.");
        RuleFor(x => x.CurrentUserId).NotEmpty().WithMessage("Current User ID is required.");
        RuleFor(x => x.CurrentUserRole).NotEmpty().WithMessage("User role is required.");
    }
}