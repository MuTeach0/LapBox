using Asp.Versioning;
using LapBox.Application.Features.Laptops.Commands.AdjustPrice;
using LapBox.Application.Features.Laptops.Commands.CreateLaptop;
using LapBox.Application.Features.Laptops.Commands.DeactivateLaptop;
using LapBox.Application.Features.Laptops.Commands.UpdateInventory;
using LapBox.Application.Features.Laptops.Commands.UpdateLaptop;
using LapBox.Application.Features.Laptops.Queries.GetLaptopById;
using LapBox.Application.Features.Laptops.Queries.GetPagedLaptops;
using LapBox.Application.Common.Models;
using LapBox.Contracts.Laptops;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using LaptopResponse = LapBox.Application.Features.Laptops.DTOs.LaptopResponse;

namespace LapBox.API.Controllers;

[Route("api/v{version:apiVersion}/laptops")]
[ApiVersion("1.0")]
[Authorize]
public sealed class LaptopsController(ISender sender) : ApiController
{
    /// <summary>
    /// Retrieves a paginated list of laptops with optional filtering.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PaginatedList<LaptopResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Gets a paginated list of laptops.")]
    [EndpointDescription("Returns laptops with optional search term and brand filter.")]
    [EndpointName("GetLaptops")]
    [MapToApiVersion("1.0")]
    [OutputCache(Duration = 60, Tags = ["laptops"])]
    public async Task<IActionResult> GetPaged(
        [FromQuery] string? searchTerm = null,
        [FromQuery] Guid? brandId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var result = await sender.Send(new GetPagedLaptopsQuery(searchTerm, brandId, page, pageSize), ct);

        return result.Match(
            response => Ok(response),
            Problem);
    }

    /// <summary>
    /// Retrieves a single laptop by ID.
    /// </summary>
    [HttpGet("{laptopId:guid}", Name = "GetLaptopById")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LaptopResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Gets a laptop by ID.")]
    [EndpointDescription("Returns detailed information about a specific laptop.")]
    [EndpointName("GetLaptopById")]
    [MapToApiVersion("1.0")]
    [OutputCache(Duration = 60, Tags = ["laptops"])]
    public async Task<IActionResult> GetById(Guid laptopId, CancellationToken ct)
    {
        var result = await sender.Send(new GetLaptopByIdQuery(laptopId), ct);

        return result.Match(
            response => Ok(response),
            Problem);
    }

    /// <summary>
    /// Creates a new laptop. Manager/Admin only.
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(LaptopResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Creates a new laptop.")]
    [EndpointDescription("Adds a new laptop to the catalog. Manager or Admin role required.")]
    [EndpointName("CreateLaptop")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> Create([FromBody] CreateLaptopRequest request, CancellationToken ct)
    {
        var command = new CreateLaptopCommand(
            request.BrandId,
            request.CategoryId,
            request.Name,
            request.Sku,
            request.Description,
            request.BasePrice,
            request.InventoryQuantity,
            request.Processor,
            request.RAM,
            request.Storage,
            request.ScreenSize,
            request.GraphicsCard);

        var result = await sender.Send(command, ct);

        return result.Match(
            response => CreatedAtRoute(
                routeName: "GetLaptopById",
                routeValues: new { version = "1.0", laptopId = response.Id },
                value: response),
            Problem);
    }

    /// <summary>
    /// Updates an existing laptop. Manager/Admin only.
    /// </summary>
    [HttpPut("{laptopId:guid}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Updates an existing laptop.")]
    [EndpointDescription("Updates laptop details. Manager or Admin role required.")]
    [EndpointName("UpdateLaptop")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> Update(Guid laptopId, [FromBody] UpdateLaptopRequest request, CancellationToken ct)
    {
        var command = new UpdateLaptopCommand(
            laptopId,
            request.BrandId,
            request.CategoryId,
            request.Name,
            request.Description,
            request.Processor,
            request.RAM,
            request.Storage,
            request.ScreenSize,
            request.GraphicsCard);

        var result = await sender.Send(command, ct);

        return result.Match(
            _ => NoContent(),
            Problem);
    }

    /// <summary>
    /// Updates laptop inventory. Manager/Admin only.
    /// </summary>
    [HttpPatch("{laptopId:guid}/inventory")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Updates laptop inventory quantity.")]
    [EndpointDescription("Adjusts the inventory quantity for a laptop. Manager or Admin role required.")]
    [EndpointName("UpdateLaptopInventory")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> UpdateInventory(Guid laptopId, [FromBody] UpdateLaptopInventoryRequest request, CancellationToken ct)
    {
        var command = new UpdateLaptopInventoryCommand(laptopId, request.Quantity);
        var result = await sender.Send(command, ct);

        return result.Match(
            _ => NoContent(),
            Problem);
    }

    /// <summary>
    /// Adjusts laptop price. Manager/Admin only.
    /// </summary>
    [HttpPatch("{laptopId:guid}/price")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Adjusts laptop price.")]
    [EndpointDescription("Changes the base price of a laptop. Manager or Admin role required.")]
    [EndpointName("AdjustLaptopPrice")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> AdjustPrice(Guid laptopId, [FromBody] AdjustLaptopPriceRequest request, CancellationToken ct)
    {
        var command = new AdjustLaptopPriceCommand(laptopId, request.NewPrice);
        var result = await sender.Send(command, ct);

        return result.Match(
            _ => NoContent(),
            Problem);
    }

    /// <summary>
    /// Deactivates a laptop. Manager/Admin only.
    /// </summary>
    [HttpDelete("{laptopId:guid}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Deactivates a laptop.")]
    [EndpointDescription("Soft-deletes a laptop by setting it as inactive. Manager or Admin role required.")]
    [EndpointName("DeactivateLaptop")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> Deactivate(Guid laptopId, CancellationToken ct)
    {
        var command = new DeactivateLaptopCommand(laptopId);
        var result = await sender.Send(command, ct);

        return result.Match(
            _ => NoContent(),
            Problem);
    }
}
