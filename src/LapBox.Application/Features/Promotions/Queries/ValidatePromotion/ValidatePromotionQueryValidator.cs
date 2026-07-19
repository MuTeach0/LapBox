using FluentValidation;
using LapBox.Application.Features.Promotions.Queries.ValidatePromotion;

namespace LapBox.Application.Features.Promotions.ValidatePromotion;

public sealed class ValidatePromotionQueryValidator : AbstractValidator<ValidatePromotionQuery>
{
    public ValidatePromotionQueryValidator()
    {
        RuleFor(x => x.Code).NotEmpty().WithMessage("Promotion code cannot be empty.");
    }
}