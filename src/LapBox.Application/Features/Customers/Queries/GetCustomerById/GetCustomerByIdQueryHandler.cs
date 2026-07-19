using LapBox.Application.Common.Interfaces.Persistence;
using LapBox.Application.Features.Customers.DTOs;
using LapBox.Application.Features.Customers.Mappers;
using LapBox.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LapBox.Application.Features.Customers.Queries.GetCustomerById;

public class GetCustomerByIdQueryHandler(ICustomerRepository repository,
    ILogger<GetCustomerByIdQueryHandler> logger)
    : IRequestHandler<GetCustomerByIdQuery, Result<CustomerDTO>>
{
    public async Task<Result<CustomerDTO>> Handle(GetCustomerByIdQuery query, CancellationToken ct)
    {
        var customer = await repository.GetByIdAsync(query.Id, ct);
        if(customer is null)
        {
            logger.LogWarning("Customer with id {CustomerId} was not found", query.Id);
            return  Error.NotFound(
                code: "Customer_NotFound",
                description: $"Customer with id '{query.Id}' was not found");
        }
        return customer.ToDTO();
    }
}