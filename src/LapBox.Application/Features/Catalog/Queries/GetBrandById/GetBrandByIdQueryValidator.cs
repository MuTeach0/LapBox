using FluentValidation;

namespace LapBox.Application.Features.Catalog.Queries.GetBrandById;

public sealed class GetBrandByIdQueryValidator : AbstractValidator<GetBrandByIdQuery>
{
    public GetBrandByIdQueryValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Brand ID is required.");
    }
}
