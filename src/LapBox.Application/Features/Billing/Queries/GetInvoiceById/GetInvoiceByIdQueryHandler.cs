using LapBox.Application.Common.Interfaces.Persistence;
using LapBox.Application.Features.Billing.DTOs;
using LapBox.Application.Features.Billing.Mappers;
using LapBox.Domain.Billing;
using LapBox.Domain.Common.Results;
using MediatR;

using Microsoft.Extensions.Logging;

namespace LapBox.Application.Features.Billing.Queries.GetInvoiceById;

public class GetInvoiceByIdQueryHandler(
    ILogger<GetInvoiceByIdQueryHandler> logger,
    IInvoiceRepository invoiceRepository)
    : IRequestHandler<GetInvoiceByIdQuery, Result<InvoiceDTO>>
{
    public async Task<Result<InvoiceDTO>> Handle(GetInvoiceByIdQuery request, CancellationToken ct)
    {
        logger.LogInformation("Fetching invoice details for ID: {InvoiceId}", request.InvoiceId);

        var invoice = await invoiceRepository.GetByIdAsync(request.InvoiceId, ct);
        if (invoice is null)
        {
            logger.LogWarning("Invoice with ID: {InvoiceId} was not found.", request.InvoiceId);
            return InvoiceErrors.NotFound;
        }

        return invoice.ToDTO();
    }
}