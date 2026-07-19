using Asp.Versioning;
using LapBox.Application.Features.Billing.Commands.MarkAsPaid;
using LapBox.Application.Features.Billing.DTOs;
using LapBox.Application.Features.Billing.Queries.GetInvoiceById;
using LapBox.Application.Features.Billing.Queries.GetInvoicePdf;
using LapBox.Contracts.Billing;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace LapBox.API.Controllers;

[Route("api/v{version:apiVersion}/billing")]
[ApiVersion("1.0")]
[Authorize]
public sealed class BillingController(ISender sender) : ApiController
{
    /// <summary>
    /// Gets an invoice by ID.
    /// </summary>
    [HttpGet("invoices/{invoiceId:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(InvoiceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Gets an invoice by ID.")]
    [EndpointDescription("Returns detailed information about a specific invoice.")]
    [EndpointName("GetInvoiceById")]
    [MapToApiVersion("1.0")]
    [OutputCache(Duration = 60, Tags = ["invoice"])]
    public async Task<IActionResult> GetInvoiceById(Guid invoiceId, CancellationToken ct)
    {
        var query = new GetInvoiceByIdQuery(invoiceId);
        var result = await sender.Send(query, ct);

        return result.Match(
            dto => Ok(MapToInvoiceResponse(dto)),
            Problem);
    }

    /// <summary>
    /// Gets an invoice as PDF.
    /// </summary>
    [HttpGet("invoices/{invoiceId:guid}/pdf")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Gets an invoice as PDF.")]
    [EndpointDescription("Generates and returns a PDF version of the invoice.")]
    [EndpointName("GetInvoicePdf")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> GetInvoicePdf(Guid invoiceId, CancellationToken ct)
    {
        var query = new GetInvoicePdfQuery(invoiceId);
        var result = await sender.Send(query, ct);

        return result.Match(
            pdf => File(pdf.Content, "application/pdf", $"invoice-{invoiceId}.pdf"),
            Problem);
    }

    /// <summary>
    /// Marks an invoice as paid. Manager/Admin only.
    /// </summary>
    [HttpPatch("invoices/{invoiceId:guid}/pay")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Marks an invoice as paid.")]
    [EndpointDescription("Updates the invoice status to paid. Manager or Admin role required.")]
    [EndpointName("MarkInvoiceAsPaid")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> MarkAsPaid(Guid invoiceId, CancellationToken ct)
    {
        var command = new MarkInvoiceAsPaidCommand(invoiceId);
        var result = await sender.Send(command, ct);

        return result.Match(
            _ => NoContent(),
            Problem);
    }

    private static InvoiceResponse MapToInvoiceResponse(InvoiceDTO dto) => new(
        dto.InvoiceId,
        dto.OrderId,
        dto.CustomerId,
        dto.Customer is not null
            ? new CustomerInvoiceInfo(dto.Customer.Name, dto.Customer.Email, dto.Customer.PhoneNumber)
            : null,
        dto.SubTotal,
        dto.TaxAmount,
        dto.TotalAmount,
        dto.PaymentStatus ?? "Pending",
        dto.PaidDateUtc,
        dto.Items.Select(i => new InvoiceLineItemResponse(
            i.InvoiceId,
            i.LineNumber,
            i.Description,
            i.Quantity,
            i.UnitPrice,
            i.LineTotal
        )).ToList()
    );
}
