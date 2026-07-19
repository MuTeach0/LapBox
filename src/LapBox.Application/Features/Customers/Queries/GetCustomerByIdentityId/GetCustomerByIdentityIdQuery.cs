using LapBox.Application.Features.Customers.DTOs;
using LapBox.Domain.Common.Results;
using MediatR;

namespace LapBox.Application.Features.Customers.Queries.GetCustomerByIdentityId;

public sealed record GetCustomerByIdentityIdQuery(Guid IdentityId) : IRequest<Result<CustomerDTO>>;
