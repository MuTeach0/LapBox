using System.Security.Claims;
using Asp.Versioning;
using LapBox.Application.Features.Customers.Command.AddCustomerAddress;
using LapBox.Application.Features.Customers.Command.RemoveCustomer;
using LapBox.Application.Features.Customers.Command.UpdateCustomer;
using LapBox.Application.Features.Customers.DTOs;
using LapBox.Application.Features.Customers.Queries.GetCustomerById;
using LapBox.Application.Features.Customers.Queries.GetCustomerByIdentityId;
using LapBox.Application.Features.Customers.Queries.GetCustomers;
using LapBox.Contracts.Customers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LapBox.API.Controllers;

[Route("api/v{version:apiVersion}/customers")]
[ApiVersion("1.0")]
[Authorize]
public sealed class CustomersController(ISender sender) : ApiController
{
    [HttpGet]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(List<CustomerDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Retrieves a paginated list of all customers.")] // 👈 تم تعديل الوصف ليكون دقيقاً
    [EndpointDescription("Returns all customers in the system with pagination support (Manager/Admin view).")]
    [EndpointName("GetCustomers")]
    [MapToApiVersion("1.0")]
    [ProducesDefaultResponseType]
    public async Task<IActionResult> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken ct = default)
    {
        var result = await sender.Send(new GetCustomersQuery(page, pageSize), ct);

        return result.Match(
            response => Ok(response),
            Problem);
    }

    [HttpGet("me")]
    [ProducesResponseType(typeof(CustomerDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Retrieves the current customer's profile.")]
    [EndpointDescription("Returns the authenticated customer's details based on the current user's identity claim.")]
    [EndpointName("GetCurrentCustomer")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> GetCurrentCustomer(CancellationToken ct)
    {
        var identityId = GetCurrentIdentityId();
        if (identityId is null)
            return Unauthorized();

        var result = await sender.Send(new GetCustomerByIdentityIdQuery(identityId.Value), ct);

        return result.Match(
            response => Ok(response),
            Problem);
    }

    [HttpPut("me")]
    [ProducesResponseType(typeof(CustomerDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Updates the current customer's profile.")]
    [EndpointDescription("Updates the authenticated customer's profile using the current identity claim.")]
    [EndpointName("UpdateCurrentCustomer")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> UpdateCurrentCustomer([FromBody] UpdateCustomerRequest request, CancellationToken ct)
    {
        var identityId = GetCurrentIdentityId();
        if (identityId is null)
            return Unauthorized();

        // 💡 نصيحة مستقبلية: يفضل دمج هذه الخطوة في الـ Handler الخاص بالـ Command مباشرة عبر تمرير الـ IdentityId
        var currentCustomerResult = await sender.Send(new GetCustomerByIdentityIdQuery(identityId.Value), ct);
        if (currentCustomerResult.IsError)
            return Problem(currentCustomerResult.Errors);

        var result = await sender.Send(
            new UpdateCustomerCommand(
                currentCustomerResult.Value.Id,
                request.Name,
                request.Email,
                request.PhoneNumber),
            ct);

        return result.Match(
            response => Ok(response),
            Problem);
    }

    [HttpPost("me/addresses")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Adds an address to the current customer.")]
    [EndpointDescription("Creates a new address for the authenticated customer.")]
    [EndpointName("AddCurrentCustomerAddress")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> AddCurrentCustomerAddress([FromBody] AddressRequest request, CancellationToken ct)
    {
        var identityId = GetCurrentIdentityId();
        if (identityId is null)
            return Unauthorized();

        // 💡 نصيحة مستقبلية: يفضل دمج هذه الخطوة في الـ Handler الخاص بالـ Command مباشرة عبر تمرير الـ IdentityId
        var currentCustomerResult = await sender.Send(new GetCustomerByIdentityIdQuery(identityId.Value), ct);
        if (currentCustomerResult.IsError)
            return Problem(currentCustomerResult.Errors);

        var result = await sender.Send(
            new AddCustomerAddressCommand(
                currentCustomerResult.Value.Id,
                request.Street,
                request.City,
                request.State,
                request.ZipCode,
                request.Country),
            ct);

        return result.Match(
            _ => StatusCode(StatusCodes.Status201Created),
            Problem);
    }

    [HttpGet("{customerId:guid}", Name = "GetCustomerById")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(CustomerDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Retrieves a customer by ID.")]
    [EndpointDescription("Returns detailed information about the specified customer if found.")]
    [EndpointName("GetCustomerById")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> GetById(Guid customerId, CancellationToken ct)
    {
        var result = await sender.Send(new GetCustomerByIdQuery(customerId), ct);
        return result.Match(
            response => Ok(response),
            Problem);
    }

    [HttpPut("{customerId:guid}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(CustomerDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Updates an existing customer.")]
    [EndpointDescription("Updates a customer and its associated details.")]
    [EndpointName("UpdateCustomer")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> Update(Guid customerId, [FromBody] UpdateCustomerRequest request, CancellationToken ct)
    {
        var command = new UpdateCustomerCommand(
            customerId,
            request.Name,
            request.Email,
            request.PhoneNumber);

        var result = await sender.Send(command, ct);

        return result.Match(
            response => Ok(response),
            Problem);
    }

    [HttpDelete("{customerId:guid}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Removes a customer.")]
    [EndpointDescription("Deletes the specified customer from the system.")]
    [EndpointName("RemoveCustomer")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> Delete(Guid customerId, CancellationToken ct)
    {
        var result = await sender.Send(new RemoveCustomerCommand(customerId), ct);

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
}