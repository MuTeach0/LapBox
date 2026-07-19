using FluentValidation;

namespace LapBox.Application.Features.Laptops.Commands.UpdateInventory;

public sealed class UpdateLaptopInventoryCommandValidator : AbstractValidator<UpdateLaptopInventoryCommand>
{
    public UpdateLaptopInventoryCommandValidator()
    {
        RuleFor(x => x.LaptopId)
            .NotEmpty().WithMessage("Laptop ID is required.");

        // التحقق من أن القيمة ليست صفراً، لأن تعديل المخزن بـ 0 ليس له قيمة منطقية
        RuleFor(x => x.QuantityChange)
            .NotEqual(0).WithMessage("Quantity change cannot be zero. Provide a positive number to restock or a negative number to reduce.");
    }
}