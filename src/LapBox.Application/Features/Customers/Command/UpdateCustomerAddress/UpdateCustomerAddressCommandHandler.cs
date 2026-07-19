using LapBox.Application.Common.Errors;
using LapBox.Application.Common.Interfaces.Persistence;
using LapBox.Domain.Common.Results;
using LapBox.Domain.Customers.ValueObjects;
using MediatR;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace LapBox.Application.Features.Customers.Command.UpdateCustomerAddress;

public class UpdateCustomerAddressCommandHandler(
    ILogger<UpdateCustomerAddressCommandHandler> logger,
    ICustomerRepository customerRepository,
    IUnitOfWork unitOfWork,
    HybridCache cache) : IRequestHandler<UpdateCustomerAddressCommand, Result<Updated>>
{
    public async Task<Result<Updated>> Handle(UpdateCustomerAddressCommand command, CancellationToken ct)
    {
        logger.LogInformation("Attempting to update address for customer ID: {CustomerId}", command.CustomerId);

        // 1. جلب العميل
        var customer = await customerRepository.GetByIdAsync(command.CustomerId, ct);
        if (customer is null)
        {
            logger.LogWarning("Address update failed. Customer {CustomerId} not found.", command.CustomerId);
            return ApplicationErrors.CustomerNotFound;
        }

        // 2. بناء كائنات الـ Value Objects (القديم للبحث والجديد للتبديل)
        // ملحوظة: مررت قيم افتراضية للـ ZipCode بناءً على معرفتنا السابقة بطلب الـ Constructor عندك
        var oldAddress = new Address(command.OldStreet, command.OldCity, "N/A", "N/A", "N/A");
        var newAddress = new Address(command.NewStreet, command.NewCity, "N/A", "N/A", command.NewCountry);

        // 3. استدعاء منطق الـ Domain
        var result = customer.UpdateAddress(oldAddress, newAddress);
        if (result.IsError)
        {
            logger.LogWarning("Domain validation failed during address update for customer {CustomerId}", command.CustomerId);
            return result.Errors;
        }

        // 4. حفظ التغييرات وتطهير الكاش
        await unitOfWork.SaveChangesAsync(ct);
        await cache.RemoveByTagAsync("customers", ct);

        logger.LogInformation("Address updated successfully for customer {CustomerId}.", command.CustomerId);

        return Result.Updated;
    }
}