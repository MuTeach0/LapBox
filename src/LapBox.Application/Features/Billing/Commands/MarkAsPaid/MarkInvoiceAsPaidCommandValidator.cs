using FluentValidation;

namespace LapBox.Application.Features.Billing.Commands.MarkAsPaid;

public sealed class MarkInvoiceAsPaidCommandValidator : AbstractValidator<MarkInvoiceAsPaidCommand>
{
    public MarkInvoiceAsPaidCommandValidator()
    {
        RuleFor(request => request.InvoiceId)
            .NotEmpty()
            .WithErrorCode("InvoiceId_Is_Required")
            .WithMessage("InvoiceId is required.");
    }
}