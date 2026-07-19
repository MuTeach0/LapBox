using Asp.Versioning;
using LapBox.Application.Features.Promotions.Command.CreatePromotion;
using LapBox.Application.Features.Promotions.DTOs;
using LapBox.Application.Features.Promotions.Queries.ValidatePromotion;
using LapBox.Contracts.Promotions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LapBox.API.Controllers;

[Route("api/v{version:apiVersion}/promotions")]
[ApiVersion("1.0")]
[Authorize]
public sealed class PromotionsController(ISender sender) : ApiController
{
    /// <summary>
    /// Validates a promotion code and returns discount info.
    /// </summary>
    [HttpPost("validate")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ValidatePromotionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [EndpointSummary("Validates a promotion code.")]
    [EndpointDescription("Checks if a promotion code is valid and returns discount details.")]
    [EndpointName("ValidatePromotion")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> Validate([FromBody] ValidatePromotionRequest request, CancellationToken ct)
    {
        var query = new ValidatePromotionQuery(request.Code);
        var result = await sender.Send(query, ct);

        return result.Match(
            promotion => Ok(new ValidatePromotionResponse(
                IsValid: true,
                Code: promotion.Code,
                DiscountAmount: 0, // Calculated elsewhere based on order
                FinalSubTotal: 0,
                ErrorMessage: null)),
            errors => Ok(new ValidatePromotionResponse(
                IsValid: false,
                Code: request.Code,
                DiscountAmount: 0,
                FinalSubTotal: request.OrderSubTotal,
                ErrorMessage: errors.Count > 0 ? errors[0].Description : "Invalid promotion code")));
    }

    /// <summary>
    /// Creates a new promotion. Manager/Admin only.
    /// </summary>
    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Creates a new promotion.")]
    [EndpointDescription("Adds a new promotion code to the system. Manager or Admin role required.")]
    [EndpointName("CreatePromotion")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> Create([FromBody] CreatePromotionRequest request, CancellationToken ct)
    {
        var command = new CreatePromotionCommand(
            request.Code,
            request.DiscountPercentage,
            request.StartDate,
            request.EndDate);

        var result = await sender.Send(command, ct);

        return result.Match(
            promotionId => StatusCode(StatusCodes.Status201Created),
            Problem);
    }
}
