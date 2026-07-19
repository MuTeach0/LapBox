using LapBox.Domain.Common.Results;
using MediatR;

namespace LapBox.Application.Features.Customers.Command.AddCustomerAddress;

public sealed record AddCustomerAddressCommand(
    Guid CustomerId,
    string Street,
    string City,
    string State,
    string ZipCode,
    string Country) : IRequest<Result<Updated>>;
