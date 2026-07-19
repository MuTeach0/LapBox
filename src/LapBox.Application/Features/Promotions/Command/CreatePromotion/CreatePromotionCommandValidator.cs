using FluentValidation;

namespace LapBox.Application.Features.Promotions.Command.CreatePromotion;

public sealed class CreatePromotionCommandValidator : AbstractValidator<CreatePromotionCommand>
{
    public CreatePromotionCommandValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(20);
        RuleFor(x => x.DiscountPercentage).InclusiveBetween(1, 100);
        RuleFor(x => x.StartDate).LessThan(x => x.EndDate).WithMessage("Start date must be before end date.");
    }
}