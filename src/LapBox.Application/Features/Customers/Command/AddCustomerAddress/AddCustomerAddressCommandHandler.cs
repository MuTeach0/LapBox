using LapBox.Application.Common.Errors;
using LapBox.Application.Common.Interfaces.Persistence;
using LapBox.Domain.Common.Results;
using LapBox.Domain.Customers.ValueObjects;
using MediatR;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace LapBox.Application.Features.Customers.Command.AddCustomerAddress;

public class AddCustomerAddressCommandHandler(
    ILogger<AddCustomerAddressCommandHandler> logger,
    ICustomerRepository customerRepository,
    IUnitOfWork unitOfWork,
    HybridCache cache) : IRequestHandler<AddCustomerAddressCommand, Result<Updated>>
{
    public async Task<Result<Updated>> Handle(AddCustomerAddressCommand command, CancellationToken ct)
    {
        logger.LogInformation("Attempting to add address for customer ID: {CustomerId}", command.CustomerId);

        var customer = await customerRepository.GetByIdAsync(command.CustomerId, ct);
        if (customer is null)
        {
            logger.LogWarning("Address add failed. Customer {CustomerId} not found.", command.CustomerId);
            return ApplicationErrors.CustomerNotFound;
        }

        var address = new Address(command.Street, command.City, command.State, command.ZipCode, command.Country);
        var result = customer.AddAddress(address);
        if (result.IsError)
        {
            logger.LogWarning("Domain validation failed while adding address for customer {CustomerId}", command.CustomerId);
            return result.Errors;
        }

        await unitOfWork.SaveChangesAsync(ct);
        await cache.RemoveByTagAsync("customers", ct);

        logger.LogInformation("Address added successfully for customer {CustomerId}.", command.CustomerId);
        return Result.Updated;
    }
}
