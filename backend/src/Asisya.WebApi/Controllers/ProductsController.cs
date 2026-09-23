using Asisya.Application.Common.Models;
using Asisya.Application.Features.Products.Commands.BulkCreateProducts;
using Asisya.Application.Features.Products.Commands.UpdateProduct;
using Asisya.Application.Features.Products.Commands.DeleteProduct;
using Asisya.Application.Features.Products.Queries.GetProductById;
using Asisya.Application.Features.Products.Queries.GetProducts;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Asisya.WebApi.Controllers;

/// <summary>
/// Controller managing product catalog search, item inspection, and high-performance bulk ingestion.
/// </summary>
[ApiController]
[Route("[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ProductsController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProductsController"/> class.
    /// </summary>
    public ProductsController(IMediator mediator, ILogger<ProductsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves a paginated, filtered, and sorted list of catalog products.
    /// Supports fuzzy text search, category filtering, and price bounds.
    /// </summary>
    /// <param name="query">Pagination and filter parameters.</param>
    /// <returns>A paginated list of product summaries.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedList<ProductSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProducts([FromQuery] GetProductsQuery query)
    {
        _logger.LogInformation("Retrieving products: Page {Page}, Search '{SearchTerm}', Category {CategoryId}",
            query.PageIndex, query.SearchTerm, query.CategoryId);

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves detailed information for a single product, including its category picture data.
    /// </summary>
    /// <param name="id">The unique product identifier.</param>
    /// <returns>Product details including category picture.</returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProductDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProductById(int id)
    {
        _logger.LogInformation("Retrieving product details for ProductId {ProductId}", id);

        var result = await _mediator.Send(new GetProductByIdQuery(id));

        if (result == null)
        {
            return NotFound(new ProblemDetails
            {
                Title = "Product Not Found",
                Detail = $"Product with ID {id} was not found in the ASISYA catalog.",
                Status = StatusCodes.Status404NotFound,
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4"
            });
        }

        return Ok(result);
    }

    /// <summary>
    /// Performs high-volume asynchronous batch ingestion of products via RabbitMQ message queue.
    /// Accessible via POST /Product or POST /Products/bulk. Requires JWT Authorization.
    /// Validates payload, enqueues BatchProductsReceivedEvent, and immediately returns HTTP 202 Accepted.
    /// </summary>
    /// <param name="command">Bulk payload or synthetic count configuration.</param>
    /// <returns>Accepted acknowledgment with correlation BatchId and queue status.</returns>
    [HttpPost]
    [HttpPost("/Product")]
    [HttpPost("bulk")]
    [Authorize]
    [ProducesResponseType(typeof(BulkCreateProductsResult), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> BulkCreateProducts([FromBody] BulkCreateProductsCommand command)
    {
        _logger.LogInformation("Enqueuing bulk product ingestion. SyntheticCount: {Count}, ExplicitCount: {ExplicitCount}",
            command.GenerateRandomCount, command.Products?.Count);

        var result = await _mediator.Send(command);
        return Accepted(result);
    }

    /// <summary>
    /// Updates an existing catalog product.
    /// Secured with JWT Authorization. Captures state mutation in AuditLogs.
    /// </summary>
    /// <param name="id">Product identifier.</param>
    /// <param name="command">Product updated fields.</param>
    /// <returns>HTTP 200 OK with confirmation payload.</returns>
    [HttpPut("{id:int}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductCommand command)
    {
        if (id != command.ProductId)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Mismatched Identifier",
                Detail = $"Route parameter ID '{id}' does not match command ProductId '{command.ProductId}'.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        _logger.LogInformation("Updating product {ProductId}: {ProductName}", id, command.ProductName);

        await _mediator.Send(command);
        return Ok(new { message = $"Product {id} updated successfully.", productId = id });
    }

    /// <summary>
    /// Deletes a catalog product.
    /// Secured with JWT Authorization. Enforces referential integrity checks and records delete action in AuditLogs.
    /// </summary>
    /// <param name="id">Product identifier to delete.</param>
    /// <returns>HTTP 200 OK with confirmation payload.</returns>
    [HttpDelete("{id:int}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        _logger.LogInformation("Deleting product {ProductId}", id);

        await _mediator.Send(new DeleteProductCommand(id));
        return Ok(new { message = $"Product {id} deleted successfully.", productId = id });
    }
}
