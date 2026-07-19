using FluentValidation;
using LapBox.Domain.Orders.Enums;

namespace LapBox.Application.Features.Orders.Command.UpdateOrder;
public sealed class UpdateOrderStatusCommandValidator : AbstractValidator<UpdateOrderStatusCommand>
{
    public UpdateOrderStatusCommandValidator()
    {
        // 1. التأكد من أن الـ OrderId ليس فارغاً
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("OrderId is required.");

        // 2. التأكد من أن الحالة المرسلة تقع ضمن قيم الـ Enum المعرفة
        RuleFor(x => x.NewStatus)
            .IsInEnum().WithMessage("Invalid order status value.");

        // 3. شرط ذكي: إذا كانت الحالة "Dispatched" (تم الشحن)، يجب إرسال الـ TrackingLabel
        RuleFor(x => x.TrackingLabel)
            .NotEmpty()
            .WithMessage("Tracking label is required when status is Dispatched.")
            .When(x => x.NewStatus == OrderStatus.Dispatched);

        // 4. شرط إضافي اختياري: منع إرسال TrackingLabel مع حالات أخرى لا تحتاجه
        RuleFor(x => x.TrackingLabel)
            .Empty()
            .WithMessage("Tracking label should only be provided when status is Dispatched.")
            .When(x => x.NewStatus != OrderStatus.Dispatched);
    }
}