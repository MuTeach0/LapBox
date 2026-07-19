using LapBox.Application.Common.Events;
using LapBox.Application.Common.Interfaces.Persistence;
using LapBox.Domain.Billing;
using LapBox.Domain.Orders.Events;
using Microsoft.Extensions.Logging;

namespace LapBox.Application.Features.Billing.Events;

public sealed class OrderCreatedEventHandler(
    IInvoiceRepository invoiceRepository,
    IUnitOfWork unitOfWork,
    ILogger<OrderCreatedEventHandler> logger) : IEventHandler<OrderCreatedDomainEvent>
{
    public async Task HandleAsync(OrderCreatedDomainEvent domainEvent, CancellationToken ct)
    {
        logger.LogInformation("Processing OrderCreatedDomainEvent to issue invoice for Order: {OrderId}", domainEvent.OrderId);

        var invoiceItems = domainEvent.Items
            .Select(item => (item.LaptopNameWithSpecs, item.Quantity, item.UnitPrice))
            .ToList();

        var invoiceResult = Invoice.Create(domainEvent.OrderId, domainEvent.UserId, invoiceItems);

        if (invoiceResult.IsError)
        {
            logger.LogError("Failed to create invoice for Order {OrderId}. Error: {Error}", 
                domainEvent.OrderId, invoiceResult.Errors.First().Description);
            return;
        }

        await invoiceRepository.AddAsync(invoiceResult.Value, ct);
        await unitOfWork.SaveChangesAsync(ct);

        logger.LogInformation("Invoice {InvoiceId} issued successfully for Order {OrderId}", 
            invoiceResult.Value.Id, domainEvent.OrderId);
    }
}
