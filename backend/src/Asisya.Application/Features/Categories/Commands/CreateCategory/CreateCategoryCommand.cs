using MediatR;

namespace Asisya.Application.Features.Categories.Commands.CreateCategory;

/// <summary>
/// Command to create a new category in the ASISYA catalog.
/// Implements requirements for POST /Category, supporting core categories such as 'SERVIDORES' and 'CLOUD'.
/// </summary>
public record CreateCategoryCommand : IRequest<int>
{
    /// <summary>
    /// Name of the category (e.g., 'SERVIDORES', 'CLOUD', 'Beverages').
    /// </summary>
    public string CategoryName { get; init; } = string.Empty;

    /// <summary>
    /// Narrative description of the category.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Binary picture data or uploaded image byte array.
    /// </summary>
    public byte[]? Picture { get; init; }
}
