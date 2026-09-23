using MediatR;

namespace Asisya.Application.Features.Categories.Commands.UpdateCategory;

/// <summary>
/// Command to update an existing category's properties in the catalog.
/// </summary>
public record UpdateCategoryCommand : IRequest<Unit>
{
    /// <summary>
    /// Identifier of the category to update.
    /// </summary>
    public int CategoryId { get; init; }

    /// <summary>
    /// Updated commercial name of the category.
    /// </summary>
    public string CategoryName { get; init; } = string.Empty;

    /// <summary>
    /// Updated narrative description.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Optional updated binary picture representation.
    /// </summary>
    public byte[]? Picture { get; init; }
}
