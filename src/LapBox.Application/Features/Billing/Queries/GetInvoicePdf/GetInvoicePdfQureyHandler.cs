
using LapBox.Application.Common.Interfaces.Persistence;
using LapBox.Application.Common.Interfaces.Services;
using LapBox.Application.Features.Billing.DTOs;
using LapBox.Domain.Billing;
using LapBox.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LapBox.Application.Features.Billing.Queries.GetInvoicePdf;

public sealed class GetInvoicePdfQueryHandler(
    IInvoiceRepository invoiceRepository,
    IInvoicePdfGenerator pdfGenerator,
    ILogger<GetInvoicePdfQueryHandler> logger) // 👈 حقن الـ Logger
    : IRequestHandler<GetInvoicePdfQuery, Result<InvoicePdfDTO>>
{
    public async Task<Result<InvoicePdfDTO>> Handle(GetInvoicePdfQuery request, CancellationToken ct)
    {
        logger.LogInformation("Generating PDF for Invoice ID: {InvoiceId}", request.InvoiceId);

        var invoice = await invoiceRepository.GetByIdAsync(request.InvoiceId, ct);
        if (invoice is null)
        {
            logger.LogWarning("PDF generation failed. Invoice {InvoiceId} not found.", request.InvoiceId);
            return InvoiceErrors.NotFound; // 👈 استخدام الخطأ المركزي
        }

        try
        {
            var pdfBytes = pdfGenerator.Generate(invoice);

            var invoicePdf = new InvoicePdfDTO
            {
                Content = pdfBytes,
                FileName = $"invoice-{invoice.Id}.pdf"
            };

            return invoicePdf;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to generate PDF for InvoiceId: {InvoiceId}", request.InvoiceId);
            return Error.Failure("An error occurred while generating the invoice PDF.");
        }
    }
}