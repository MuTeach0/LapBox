using LapBox.Domain.Common.Results;
using MediatR;

namespace LapBox.Application.Features.Billing.Commands.MarkAsPaid;

public sealed record MarkInvoiceAsPaidCommand(Guid InvoiceId) : IRequest<Result<Success>>;