using LapBox.Domain.Common.Results;
using MediatR;

namespace LapBox.Application.Features.Customers.Command.CreateCustomer;

public sealed record CreateCustomerCommand(
    Guid IdentityId,
    string Name,
    string Email,
    string? PhoneNumber) : IRequest<Result<Guid>>;