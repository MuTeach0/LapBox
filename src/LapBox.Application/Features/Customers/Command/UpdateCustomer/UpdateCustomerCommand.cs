using LapBox.Domain.Common.Results;
using MediatR;

namespace LapBox.Application.Features.Customers.Command.UpdateCustomer;

public sealed record UpdateCustomerCommand(
    Guid CustomerId,
    string Name,
    string Email,
    string? PhoneNumber
) : IRequest<Result<Updated>>;