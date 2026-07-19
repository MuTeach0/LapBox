using System.Security.Claims;
using Asp.Versioning;
using LapBox.Application.Features.Customers.Queries.GetCustomerByIdentityId;
using LapBox.Application.Features.Reviews.Command.AddReview;
using LapBox.Application.Features.Reviews.DTOs;
using LapBox.Application.Features.Reviews.Queries.GetLaptopReviews;
using LapBox.Contracts.Reviews;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace LapBox.API.Controllers;

[Route("api/v{version:apiVersion}/reviews")]
[ApiVersion("1.0")]
[Authorize]
public sealed class ReviewsController(ISender sender) : ApiController
{
    /// <summary>
    /// Gets all reviews for a specific laptop.
    /// </summary>
    [HttpGet("laptop/{laptopId:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<ReviewResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Gets reviews for a laptop.")]
    [EndpointDescription("Returns all reviews for a specific laptop product.")]
    [EndpointName("GetLaptopReviews")]
    [MapToApiVersion("1.0")]
    [OutputCache(Duration = 60, Tags = ["reviews"])]
    public async Task<IActionResult> GetLaptopReviews(Guid laptopId, CancellationToken ct)
    {
        var query = new GetLaptopReviewsQuery(laptopId);
        var result = await sender.Send(query, ct);

        return result.Match(
            reviews => Ok(reviews.Select(r => new ReviewResponse(
                r.Id,
                r.LaptopId,
                r.UserName,
                r.Rating,
                r.Comment,
                r.CreatedOnUtc
            )).ToList()),
            Problem);
    }

    /// <summary>
    /// Adds a review for a laptop.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Adds a review for a laptop.")]
    [EndpointDescription("Creates a new review for a laptop product. User must be authenticated.")]
    [EndpointName("AddReview")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> AddReview([FromBody] AddReviewRequest request, CancellationToken ct)
    {
        var identityId = GetCurrentIdentityId();
        if (identityId is null)
            return Unauthorized();

        var customerResult = await sender.Send(new GetCustomerByIdentityIdQuery(identityId.Value), ct);
        if (customerResult.IsError)
            return Problem(customerResult.Errors);

        var command = new AddReviewCommand(request.LaptopId, request.Rating, request.Comment);
        var result = await sender.Send(command, ct);

        return result.Match(
            reviewId => StatusCode(StatusCodes.Status201Created),
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
