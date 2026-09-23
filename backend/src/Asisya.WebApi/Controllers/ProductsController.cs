using Asisya.Application.Common.Models;
using Asisya.Application.Features.Products.Commands.BulkCreateProducts;
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
    /// Performs high-volume transactional streaming batch ingestion of products.
    /// Accessible via POST /Product or POST /Products/bulk. Requires JWT Authorization.
    /// Supports explicit product payloads or synthetic generation of up to 100,000 items.
    /// </summary>
    /// <param name="command">Bulk payload or synthetic count configuration.</param>
    /// <returns>Execution summary with metrics on imported vs failed records.</returns>
    [HttpPost]
    [HttpPost("/Product")]
    [HttpPost("bulk")]
    [Authorize]
    [ProducesResponseType(typeof(BulkCreateProductsResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> BulkCreateProducts([FromBody] BulkCreateProductsCommand command)
    {
        _logger.LogInformation("Initiating bulk product ingestion. SyntheticCount: {Count}, ExplicitCount: {ExplicitCount}",
            command.GenerateRandomCount, command.Products?.Count);

        var result = await _mediator.Send(command);
        return Ok(result);
    }
}
