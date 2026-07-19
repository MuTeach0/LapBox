using LapBox.Application.Common.Errors;
using LapBox.Application.Common.Interfaces.Persistence;
using LapBox.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace LapBox.Application.Features.Customers.Command.UpdateCustomer;

public class UpdateCustomerCommandHandler(
    ILogger<UpdateCustomerCommandHandler> logger,
    ICustomerRepository customerRepository,
    IUnitOfWork unitOfWork,
    HybridCache cache) : IRequestHandler<UpdateCustomerCommand, Result<Updated>>
{
    public async Task<Result<Updated>> Handle(UpdateCustomerCommand command, CancellationToken ct)
    {
        logger.LogInformation("Attempting to update profile for customer ID: {CustomerId}", command.CustomerId);

        // 1. جلب العميل من المستودع
        var customer = await customerRepository.GetByIdAsync(command.CustomerId, ct);
        if (customer is null)
        {
            logger.LogWarning("Update failed. Customer with ID: {CustomerId} was not found.", command.CustomerId);
            return ApplicationErrors.CustomerNotFound;
        }

        // 2. استدعاء ميثود البزنس من الـ Domain وتمرير البيانات الجديدة
        var updateResult = customer.UpdateProfile(command.Name, command.Email, command.PhoneNumber);
        if (updateResult.IsError)
        {
            logger.LogWarning("Domain validation failed during customer update for ID: {CustomerId}", command.CustomerId);
            return updateResult.Errors;
        }

        await unitOfWork.SaveChangesAsync(ct);

        await cache.RemoveByTagAsync("customers", ct);

        logger.LogInformation("Customer profile with ID: {CustomerId} updated successfully and cache evicted.", command.CustomerId);

        return Result.Updated;
    }
}