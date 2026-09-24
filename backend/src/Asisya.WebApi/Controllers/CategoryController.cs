using Asisya.Application.Features.Categories.Commands.CreateCategory;
using Asisya.Application.Features.Categories.Commands.DeleteCategory;
using Asisya.Application.Features.Categories.Commands.UpdateCategory;
using Asisya.Application.Features.Categories.Queries.GetCategories;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Asisya.WebApi.Controllers;

/// <summary>
/// Controller managing product categories in the ASISYA catalog.
/// Implements full CRUD capabilities: GET, POST, PUT, DELETE /Category.
/// </summary>
[ApiController]
[Route("[controller]")]
public class CategoryController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<CategoryController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="CategoryController"/> class.
    /// </summary>
    public CategoryController(IMediator mediator, ILogger<CategoryController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves all categories in the catalog with product counts.
    /// Secured with JWT authorization.
    /// </summary>
    /// <returns>A list of catalog categories.</returns>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(List<CategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<CategoryDto>>> GetCategories()
    {
        _logger.LogInformation("Retrieving catalog categories");
        var categories = await _mediator.Send(new GetCategoriesQuery());
        return Ok(categories);
    }

    /// <summary>
    /// Creates a new category in the catalog.
    /// Secured with JWT authorization. Automatically normalizes core categories ('SERVIDORES', 'CLOUD').
    /// </summary>
    /// <param name="command">Category payload including name, description, and optional picture bytes.</param>
    /// <returns>The newly created or existing CategoryId.</returns>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryCommand command)
    {
        _logger.LogInformation("Processing category creation for name: {CategoryName}", command.CategoryName);

        var categoryId = await _mediator.Send(command);

        return CreatedAtAction(nameof(CreateCategory), new { id = categoryId }, new
        {
            categoryId,
            message = "Category created or resolved successfully."
        });
    }

    /// <summary>
    /// Updates an existing category in the catalog.
    /// Secured with JWT authorization.
    /// </summary>
    /// <param name="id">The unique CategoryId to update.</param>
    /// <param name="command">Updated category data.</param>
    /// <returns>HTTP 204 NoContent upon successful update.</returns>
    [HttpPut("{id:int}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateCategoryCommand command)
    {
        if (id != command.CategoryId)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Mismatched Identifier",
                Detail = $"Route parameter ID ({id}) does not match body CategoryId ({command.CategoryId})."
            });
        }

        _logger.LogInformation("Updating category ID: {CategoryId}", id);
        await _mediator.Send(command);
        return NoContent();
    }

    /// <summary>
    /// Deletes a category from the catalog.
    /// Enforces referential integrity (ON DELETE RESTRICT): will fail if products reference this category.
    /// Secured with JWT authorization.
    /// </summary>
    /// <param name="id">The unique CategoryId to delete.</param>
    /// <returns>HTTP 204 NoContent upon successful deletion.</returns>
    [HttpDelete("{id:int}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        _logger.LogInformation("Attempting to delete category ID: {CategoryId}", id);
        await _mediator.Send(new DeleteCategoryCommand(id));
        return NoContent();
    }
}
