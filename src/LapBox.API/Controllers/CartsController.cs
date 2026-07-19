using System.Security.Claims;
using Asp.Versioning;
using LapBox.Application.Features.Carts.Command.AddOrUpdateCartItem;
using LapBox.Application.Features.Carts.Command.ClearCart;
using LapBox.Application.Features.Carts.Command.RemoveCartItem;
using LapBox.Application.Features.Carts.DTOs;
using LapBox.Application.Features.Carts.Queries.GetCartByIdentityId;
using LapBox.Application.Features.Laptops.Queries.GetLaptopById;
using LapBox.Contracts.Carts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LapBox.API.Controllers;

[Route("api/v{version:apiVersion}/cart")]
[ApiVersion("1.0")]
[Authorize]
public sealed class CartsController(ISender sender) : ApiController
{
    /// <summary>
    /// Gets the current user's cart.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(CartResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Gets the current user's cart.")]
    [EndpointDescription("Returns the authenticated user's shopping cart with all items.")]
    [EndpointName("GetCart")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> GetCart(CancellationToken ct)
    {
        var identityId = GetCurrentIdentityId();
        if (identityId is null)
            return Unauthorized();

        var result = await sender.Send(new GetCartByIdentityIdQuery(identityId.Value), ct);

        return result.Match(
            response => Ok(MapToCartResponse(response)),
            Problem);
    }

    /// <summary>
    /// Adds or updates an item in the cart.
    /// </summary>
    [HttpPost("items")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Adds or updates a cart item.")]
    [EndpointDescription("Adds a laptop to the cart or updates quantity if it already exists.")]
    [EndpointName("AddOrUpdateCartItem")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> AddOrUpdateItem([FromBody] AddOrUpdateCartItemRequest request, CancellationToken ct)
    {
        var identityId = GetCurrentIdentityId();
        if (identityId is null)
            return Unauthorized();

        // Get the laptop to verify it exists and get current price
        var laptopResult = await sender.Send(new GetLaptopByIdQuery(request.LaptopId), ct);
        if (laptopResult.IsError)
            return Problem(laptopResult.Errors);

        var command = new AddOrUpdateCartItemCommand(
            identityId.Value,
            request.LaptopId,
            request.Quantity,
            laptopResult.Value.BasePrice);

        var result = await sender.Send(command, ct);

        return result.Match(
            _ => StatusCode(StatusCodes.Status201Created),
            Problem);
    }

    /// <summary>
    /// Removes an item from the cart.
    /// </summary>
    [HttpDelete("items/{laptopId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Removes an item from the cart.")]
    [EndpointDescription("Removes a specific laptop from the current user's cart.")]
    [EndpointName("RemoveCartItem")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> RemoveItem(Guid laptopId, CancellationToken ct)
    {
        var identityId = GetCurrentIdentityId();
        if (identityId is null)
            return Unauthorized();

        var command = new RemoveCartItemCommand(identityId.Value, laptopId);
        var result = await sender.Send(command, ct);

        return result.Match(
            _ => NoContent(),
            Problem);
    }

    /// <summary>
    /// Clears the entire cart.
    /// </summary>
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Clears the cart.")]
    [EndpointDescription("Removes all items from the current user's cart.")]
    [EndpointName("ClearCart")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> ClearCart(CancellationToken ct)
    {
        var identityId = GetCurrentIdentityId();
        if (identityId is null)
            return Unauthorized();

        var command = new ClearCartCommand(identityId.Value);
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

    private static CartResponse MapToCartResponse(CartDTO dto) => new(
        dto.Id,
        dto.IdentityId,
        dto.Status,
        dto.Items.Select(i => new CartItemResponse(
            i.Id,
            i.LaptopId,
            i.Quantity,
            i.UnitPrice,
            i.TotalPrice
        )).ToList(),
        dto.TotalPrice
    );
}
