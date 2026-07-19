using LapBox.Application.Common.Interfaces.Persistence;
using LapBox.Application.Features.Customers.DTOs;
using LapBox.Application.Features.Customers.Mappers;
using LapBox.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LapBox.Application.Features.Customers.Queries.GetCustomerByIdentityId;

public class GetCustomerByIdentityIdQueryHandler(
    ICustomerRepository repository,
    ILogger<GetCustomerByIdentityIdQueryHandler> logger)
    : IRequestHandler<GetCustomerByIdentityIdQuery, Result<CustomerDTO>>
{
    public async Task<Result<CustomerDTO>> Handle(GetCustomerByIdentityIdQuery query, CancellationToken ct)
    {
        var customer = await repository.GetByIdentityIdAsync(query.IdentityId, ct);
        if (customer is null)
        {
            logger.LogWarning("Customer with identity id {IdentityId} was not found", query.IdentityId);
            return Error.NotFound(
                code: "Customer_NotFound",
                description: $"Customer with identity id '{query.IdentityId}' was not found");
        }

        return customer.ToDTO();
    }
}
