using FluentValidation;

namespace LapBox.Application.Features.Reviews.Command.AddReview;

public sealed class AddReviewCommandValidator : AbstractValidator<AddReviewCommand>
{
    public AddReviewCommandValidator()
    {
        RuleFor(x => x.LaptopId).NotEmpty();
        RuleFor(x => x.Rating).InclusiveBetween(1, 5).WithMessage("Rating must be between 1 and 5.");
        RuleFor(x => x.Comment).NotEmpty().MaximumLength(500);
    }
}