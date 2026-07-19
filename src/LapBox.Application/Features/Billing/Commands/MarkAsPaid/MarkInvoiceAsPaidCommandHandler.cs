using LapBox.Application.Common.Interfaces.Persistence;
using LapBox.Domain.Billing;
using LapBox.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace LapBox.Application.Features.Billing.Commands.MarkAsPaid;

public sealed class MarkInvoiceAsPaidCommandHandler(
    IInvoiceRepository invoiceRepository,
    IUnitOfWork unitOfWork,
    ILogger<MarkInvoiceAsPaidCommandHandler> logger, // إضافة الـ Logger
    HybridCache cache) // إضافة الـ Cache
    : IRequestHandler<MarkInvoiceAsPaidCommand, Result<Success>>
{
    public async Task<Result<Success>> Handle(MarkInvoiceAsPaidCommand request, CancellationToken ct)
    {
        var invoice = await invoiceRepository.GetByIdAsync(request.InvoiceId, ct);
        if (invoice is null) return InvoiceErrors.NotFound;

        var result = invoice.MarkAsPaid();
        if (result.IsError) return result;

        await unitOfWork.SaveChangesAsync(ct);

        // تسجيل العملية
        logger.LogInformation("Invoice {Id} marked as paid.", request.InvoiceId);

        // تطهير الكاش فوراً
        await cache.RemoveByTagAsync($"invoice", ct);
        // await cache.RemoveAsync($"invoice_order_{invoice.OrderId}", ct);

        return Result.Success;
    }
}