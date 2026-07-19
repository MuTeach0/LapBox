using LapBox.Contracts.Orders;
using LapBox.Domain.Common.Results;
using MediatR;

namespace LapBox.Application.Features.Orders.Command.CreateOrder;

/// <summary>
/// Creates an Order from temporary stock reservations.
/// 
/// FLOW: Register (no role) → PlaceOrder (reserve) → Payment → CreateOrder (finalize)
/// 
/// This command:
/// - Finds existing active reservations for the user (by IdentityId)
/// - Creates Customer record if user doesn't have one (first order ever)
/// - Changes role from no role to "Customer"
/// - Creates the Order, and consumes the reservations
public sealed record CreateOrderCommand(
    Guid IdentityId,
    string ShippingStreet,
    string ShippingCity,
    string ShippingCountry,
    string ShippingState,
    string ShippingZipCode) : IRequest<Result<CreateOrderResponse>>;
