using LapBox.Application.Features.Billing.DTOs;
using LapBox.Domain.Common.Results;

using MediatR;

namespace LapBox.Application.Features.Billing.Queries.GetInvoicePdf;

public sealed record GetInvoicePdfQuery(Guid InvoiceId) : IRequest<Result<InvoicePdfDTO>>
{
}