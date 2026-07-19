using FluentValidation;

namespace LapBox.Application.Features.Catalog.Command.DeactivateBrand;

public sealed class DeactivateBrandCommandValidator : AbstractValidator<DeactivateBrandCommand>
{
    public DeactivateBrandCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Brand ID is required.");
    }
}