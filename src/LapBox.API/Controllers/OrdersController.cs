using System.Security.Claims;
using Asp.Versioning;
using LapBox.Application.Features.Orders.Command.CreateOrder;
using LapBox.Application.Features.Orders.Command.RemoveOrder;
using LapBox.Application.Features.Orders.Command.UpdateOrder;
using LapBox.Application.Features.Orders.DTOs;
using LapBox.Application.Features.Orders.Queries.GetOrderById;
using LapBox.Application.Features.Orders.Queries.GetOrders;
using LapBox.Application.Features.Orders.Queries.GetOrdersByUserId;
using LapBox.Domain.Common.Results;
using LapBox.Domain.Orders.Enums;
using LapBox.Contracts.Orders;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LapBox.API.Controllers;

[Route("api/v{version:apiVersion}/orders")]
[ApiVersion("1.0")]
[Authorize]
public sealed class OrdersController(ISender sender) : ApiController
{
    /// <summary>
    /// Retrieves the current user's orders.
    /// </summary>
    [HttpGet("my-orders")]
    [ProducesResponseType(typeof(List<OrderSummaryDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Gets the current user's orders.")]
    [EndpointDescription("Returns all orders for the authenticated user.")]
    [EndpointName("GetMyOrders")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> GetMyOrders(CancellationToken ct)
    {
        var identityId = GetCurrentIdentityId();
        if (identityId is null)
            return Unauthorized();

        var role = GetCurrentUserRole();

        var result = await sender.Send(new GetOrdersByUserIdQuery(identityId.Value, identityId.Value, role), ct);

        return result.Match(
            response => Ok(response),
            Problem);
    }

    /// <summary>
    /// Retrieves all orders. Manager/Admin only.
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(List<OrderSummaryDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Gets all orders (Admin/Manager).")]
    [EndpointDescription("Returns all orders in the system. Manager or Admin role required.")]
    [EndpointName("GetOrders")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var identityId = GetCurrentIdentityId();
        if (identityId is null)
            return Unauthorized();

        var role = GetCurrentUserRole();

        var result = await sender.Send(new GetOrdersQuery(page, pageSize, identityId.Value, role), ct);

        return result.Match(
            response => Ok(response),
            Problem);
    }

    /// <summary>
    /// Retrieves an order by ID.
    /// </summary>
    [HttpGet("{orderId:guid}", Name = "GetOrderById")]
    [ProducesResponseType(typeof(OrderDetailsDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Gets an order by ID.")]
    [EndpointDescription("Returns detailed information about a specific order.")]
    [EndpointName("GetOrderById")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> GetById(Guid orderId, CancellationToken ct)
    {
        var identityId = GetCurrentIdentityId();
        if (identityId is null)
            return Unauthorized();

        var role = GetCurrentUserRole();

        var result = await sender.Send(new GetOrderByIdQuery(orderId, identityId.Value, role), ct);

        return result.Match(
            response => Ok(response),
            Problem);
    }

    /// <summary>
    /// Creates an order from temporary reservations.
    /// Call this after PlaceOrder succeeds and payment is confirmed.
    /// 
    /// For first-time customers (User with no role), this will:
    /// - Create the Customer record
    /// - Add "Customer" role to the user
    /// - Create the order from reservations
    /// </summary>
    [HttpPost("create")]
    [ApiExplorerSettings(IgnoreApi = false)]
    [ProducesResponseType(typeof(CreateOrderResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Creates an order from temporary reservations.")]
    [EndpointDescription("Finalizes an order by consuming existing temporary reservations. Call after payment confirmation. Creates Customer record for first-time buyers.")]
    [EndpointName("CreateOrder")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request, CancellationToken ct)
    {
        var identityId = GetCurrentIdentityId();
        if (identityId is null)
            return Unauthorized();

        // Pass IdentityId directly - handler will create Customer if needed
        var command = new CreateOrderCommand(
            identityId.Value, // Use IdentityId directly
            request.ShippingStreet,
            request.ShippingCity,
            request.ShippingCountry,
            request.ShippingState,
            request.ShippingZipCode);

        var result = await sender.Send(command, ct);

        return result.Match(
            response => CreatedAtRoute(
                routeName: "GetOrderById",
                routeValues: new { version = "1.0", orderId = response.OrderId },
                value: response),
            Problem);
    }

    /// <summary>
    /// Updates order status. Manager/Admin only.
    /// </summary>
    [HttpPatch("{orderId:guid}/status")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Updates order status.")]
    [EndpointDescription("Changes the status of an order. Manager or Admin role required.")]
    [EndpointName("UpdateOrderStatus")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> UpdateStatus(
        Guid orderId,
        [FromBody] UpdateOrderStatusRequest request,
        CancellationToken ct)
    {
        if (!Enum.TryParse<OrderStatus>(request.Status, true, out var newStatus))
        {
            var error = Error.Validation("Order.InvalidStatus",
                "Invalid order status. Valid values: Placed, InventoryReserved, Packaged, Dispatched, Delivered, Cancelled");
            return Problem([error]);
        }

        var command = new UpdateOrderStatusCommand(orderId, newStatus);
        var result = await sender.Send(command, ct);

        return result.Match(
            _ => NoContent(),
            Problem);
    }

    /// <summary>
    /// Removes an order. Manager/Admin only.
    /// </summary>
    [HttpDelete("{orderId:guid}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Removes an order.")]
    [EndpointDescription("Deletes an order from the system. Manager or Admin role required.")]
    [EndpointName("RemoveOrder")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> Delete(Guid orderId, CancellationToken ct)
    {
        var identityId = GetCurrentIdentityId();
        if (identityId is null)
            return Unauthorized();

        var role = GetCurrentUserRole();

        var command = new RemoveOrderCommand(orderId, identityId.Value, role);
        var result = await sender.Send(command, ct);

        return result.Match(
            _ => NoContent(),
            Problem);
    }

    private Guid? GetCurrentIdentityId()
    {
        var identityIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return string.IsNullOrWhiteSpace(identityIdClaim) || !Guid.TryParse(identityIdClaim, out var identityId)
            ? null
            : identityId;
    }

    private string GetCurrentUserRole()
    {
        return User.FindFirstValue(ClaimTypes.Role) ?? "Customer";
    }
}
