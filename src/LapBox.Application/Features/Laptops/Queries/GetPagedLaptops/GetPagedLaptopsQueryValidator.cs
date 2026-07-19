using FluentValidation;

namespace LapBox.Application.Features.Laptops.Queries.GetPagedLaptops;

public sealed class GetPagedLaptopsQueryValidator : AbstractValidator<GetPagedLaptopsQuery>
{
    public GetPagedLaptopsQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("Page number must be at least 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100.");
    }
}