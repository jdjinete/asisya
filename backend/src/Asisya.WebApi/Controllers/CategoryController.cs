using Asisya.Application.Features.Categories.Commands.CreateCategory;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Asisya.WebApi.Controllers;

/// <summary>
/// Controller managing product categories in the ASISYA catalog.
/// Implements requirements for POST /Category.
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
}
