using FluentValidation;

namespace LapBox.Application.Features.Carts.Queries.GetCartByIdentityId;

public sealed class GetCartByIdentityIdQueryValidator : AbstractValidator<GetCartByIdentityIdQuery>
{
    public GetCartByIdentityIdQueryValidator()
    {
        RuleFor(x => x.IdentityId)
            .NotEmpty().WithMessage("Identity identifier is required.");
    }
}