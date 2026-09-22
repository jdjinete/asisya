using Asisya.Application.Common.Models;
using MediatR;

namespace Asisya.Application.Features.Products.Queries.GetProducts;

/// <summary>
/// Query to retrieve a filtered, sorted, and server-side paginated list of catalog products.
/// Satisfies all operational requirements for GET /Products.
/// </summary>
public record GetProductsQuery : IRequest<PaginatedList<ProductSummaryDto>>
{
    /// <summary>
    /// Current 1-based page index (default: 1).
    /// </summary>
    public int PageIndex { get; init; } = 1;

    /// <summary>
    /// Quantity of items per page (default: 10, max: 100).
    /// </summary>
    public int PageSize { get; init; } = 10;

    /// <summary>
    /// Optional textual search query for fuzzy/prefix filtering on ProductName.
    /// </summary>
    public string? SearchTerm { get; init; }

    /// <summary>
    /// Optional foreign key category filter.
    /// </summary>
    public int? CategoryId { get; init; }

    /// <summary>
    /// Optional minimum unit price threshold.
    /// </summary>
    public decimal? MinPrice { get; init; }

    /// <summary>
    /// Optional maximum unit price threshold.
    /// </summary>
    public decimal? MaxPrice { get; init; }

    /// <summary>
    /// Optional filter for discontinued products.
    /// </summary>
    public bool? Discontinued { get; init; }

    /// <summary>
    /// Sort column identifier ('name', 'price', 'stock', default: 'name').
    /// </summary>
    public string? SortBy { get; init; } = "name";

    /// <summary>
    /// Sort ordering direction ('asc' or 'desc', default: 'asc').
    /// </summary>
    public string? SortOrder { get; init; } = "asc";
}
