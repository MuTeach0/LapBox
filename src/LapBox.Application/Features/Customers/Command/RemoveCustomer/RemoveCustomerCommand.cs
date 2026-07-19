using LapBox.Domain.Common.Results;
using MediatR;

namespace LapBox.Application.Features.Customers.Command.RemoveCustomer;

public sealed record RemoveCustomerCommand(Guid CustomerId)
    : IRequest<Result<Deleted>>;