using Asp.Versioning;
using LapBox.Application.Features.Catalog.Command.CreateBrand;
using LapBox.Application.Features.Catalog.Command.CreateCategory;
using LapBox.Application.Features.Catalog.Command.DeactivateBrand;
using LapBox.Application.Features.Catalog.Command.DeactivateCategory;
using LapBox.Application.Features.Catalog.Command.UpdateBrand;
using LapBox.Application.Features.Catalog.Command.UpdateCategory;
using LapBox.Application.Features.Catalog.Queries.GetAllBrands;
using LapBox.Application.Features.Catalog.Queries.GetAllCategories;
using LapBox.Application.Features.Catalog.Queries.GetBrandById;
using LapBox.Application.Features.Catalog.Queries.GetCategoryById;
using LapBox.Contracts.Catalog;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using BrandResponse = LapBox.Application.Features.Catalog.DTOs.BrandResponse;
using CategoryResponse = LapBox.Application.Features.Catalog.DTOs.CategoryResponse;

namespace LapBox.API.Controllers;

[Route("api/v{version:apiVersion}/catalog")]
[ApiVersion("1.0")]
[Authorize]
public sealed class CatalogController(ISender sender) : ApiController
{
    #region Brands

    /// <summary>
    /// Gets all active brands.
    /// </summary>
    [HttpGet("brands")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<BrandResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Gets all active brands.")]
    [EndpointDescription("Returns all brands that are currently active.")]
    [EndpointName("GetBrands")]
    [MapToApiVersion("1.0")]
    
    public async Task<IActionResult> GetBrands(CancellationToken ct)
    {
        var result = await sender.Send(new GetAllBrandsQuery(), ct);

        return result.Match(
            response => Ok(response),
            Problem);
    }

    /// <summary>
    /// Gets a brand by ID.
    /// </summary>
    [HttpGet("brands/{brandId:guid}", Name = "GetBrandById")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(BrandResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Gets a brand by ID.")]
    [EndpointDescription("Returns detailed information about a specific brand.")]
    [EndpointName("GetBrandById")]
    [MapToApiVersion("1.0")]
    
    public async Task<IActionResult> GetBrandById(Guid brandId, CancellationToken ct)
    {
        var result = await sender.Send(new GetBrandByIdQuery(brandId), ct);

        return result.Match(
            response => Ok(response),
            Problem);
    }

    /// <summary>
    /// Creates a new brand. Manager/Admin only.
    /// </summary>
    [HttpPost("brands")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(BrandResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Creates a new brand.")]
    [EndpointDescription("Adds a new brand to the catalog. Manager or Admin role required.")]
    [EndpointName("CreateBrand")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> CreateBrand([FromBody] CreateBrandRequest request, CancellationToken ct)
    {
        var command = new CreateBrandCommand(request.Name, request.Description, request.LogoUrl);
        var result = await sender.Send(command, ct);

        return result.Match(
            response => CreatedAtRoute(
                routeName: "GetBrandById",
                routeValues: new { version = "1.0", brandId = response.Id },
                value: response),
            Problem);
    }

    /// <summary>
    /// Updates an existing brand. Manager/Admin only.
    /// </summary>
    [HttpPut("brands/{brandId:guid}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(BrandResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Updates a brand.")]
    [EndpointDescription("Updates brand details. Manager or Admin role required.")]
    [EndpointName("UpdateBrand")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> UpdateBrand(Guid brandId, [FromBody] UpdateBrandRequest request, CancellationToken ct)
    {
        var command = new UpdateBrandCommand(brandId, request.Name, request.Description, request.LogoUrl);
        var result = await sender.Send(command, ct);

        return result.Match(
            response => Ok(response),
            Problem);
    }

    /// <summary>
    /// Deactivates a brand. Manager/Admin only.
    /// </summary>
    [HttpDelete("brands/{brandId:guid}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Deactivates a brand.")]
    [EndpointDescription("Soft-deletes a brand by setting it as inactive. Manager or Admin role required.")]
    [EndpointName("DeactivateBrand")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> DeactivateBrand(Guid brandId, CancellationToken ct)
    {
        var command = new DeactivateBrandCommand(brandId);
        var result = await sender.Send(command, ct);

        return result.Match(
            _ => NoContent(),
            Problem);
    }

    #endregion

    #region Categories

    /// <summary>
    /// Gets all active categories.
    /// </summary>
    [HttpGet("categories")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(List<CategoryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Gets all active categories.")]
    [EndpointDescription("Returns all categories that are currently active.")]
    [EndpointName("GetCategories")]
    [MapToApiVersion("1.0")]
    
    public async Task<IActionResult> GetCategories(CancellationToken ct)
    {
        var result = await sender.Send(new GetAllCategoriesQuery(), ct);

        return result.Match(
            response => Ok(response),
            Problem);
    }

    /// <summary>
    /// Gets a category by ID.
    /// </summary>
    [HttpGet("categories/{categoryId:guid}", Name = "GetCategoryById")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Gets a category by ID.")]
    [EndpointDescription("Returns detailed information about a specific category.")]
    [EndpointName("GetCategoryById")]
    [MapToApiVersion("1.0")]
    
    public async Task<IActionResult> GetCategoryById(Guid categoryId, CancellationToken ct)
    {
        var result = await sender.Send(new GetCategoryByIdQuery(categoryId), ct);

        return result.Match(
            response => Ok(response),
            Problem);
    }

    /// <summary>
    /// Creates a new category. Manager/Admin only.
    /// </summary>
    [HttpPost("categories")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Creates a new category.")]
    [EndpointDescription("Adds a new category to the catalog. Manager or Admin role required.")]
    [EndpointName("CreateCategory")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequest request, CancellationToken ct)
    {
        var command = new CreateCategoryCommand(request.Name, request.Description);
        var result = await sender.Send(command, ct);

        return result.Match(
            response => StatusCode(StatusCodes.Status201Created),
            Problem);
    }

    /// <summary>
    /// Updates an existing category. Manager/Admin only.
    /// </summary>
    [HttpPut("categories/{categoryId:guid}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Updates a category.")]
    [EndpointDescription("Updates category details. Manager or Admin role required.")]
    [EndpointName("UpdateCategory")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> UpdateCategory(Guid categoryId, [FromBody] UpdateCategoryRequest request, CancellationToken ct)
    {
        var command = new UpdateCategoryCommand(categoryId, request.Name, request.Description!);
        var result = await sender.Send(command, ct);

        return result.Match(
            response => Ok(response),
            Problem);
    }

    /// <summary>
    /// Deactivates a category. Manager/Admin only.
    /// </summary>
    [HttpDelete("categories/{categoryId:guid}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Deactivates a category.")]
    [EndpointDescription("Soft-deletes a category by setting it as inactive. Manager or Admin role required.")]
    [EndpointName("DeactivateCategory")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> DeactivateCategory(Guid categoryId, CancellationToken ct)
    {
        var command = new DeactivateCategoryCommand(categoryId);
        var result = await sender.Send(command, ct);

        return result.Match(
            _ => NoContent(),
            Problem);
    }

    #endregion
}
