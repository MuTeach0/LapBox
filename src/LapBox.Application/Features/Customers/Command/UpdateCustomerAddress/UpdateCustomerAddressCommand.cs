using LapBox.Domain.Common.Results;
using MediatR;

namespace LapBox.Application.Features.Customers.Command.UpdateCustomerAddress;

public sealed record UpdateCustomerAddressCommand(
    Guid CustomerId,
    string OldStreet,
    string OldCity,
    string NewStreet,
    string NewCity,
    string NewCountry) : IRequest<Result<Updated>>;