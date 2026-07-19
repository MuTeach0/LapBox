using FluentValidation;

namespace LapBox.Application.Features.Laptops.Queries.GetLaptopById;

public sealed class GetLaptopByIdQueryValidator : AbstractValidator<GetLaptopByIdQuery>
{
    public GetLaptopByIdQueryValidator()
    {
        RuleFor(x => x.LaptopId)
            .NotEmpty().WithMessage("Laptop ID is required.");
    }
}