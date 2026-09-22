using MediatR;

namespace Asisya.Application.Features.Products.Commands.BulkCreateProducts;

/// <summary>
/// Command to ingest a high volume of products into the catalog using a streaming batch transaction.
/// Supports both explicit payload uploads and high-speed synthetic generation (e.g. 100,000 items)
/// categorized under 'SERVIDORES' and 'CLOUD' for load verification.
/// </summary>
public record BulkCreateProductsCommand : IRequest<BulkCreateProductsResult>
{
    /// <summary>
    /// Explicit collection of product items to insert.
    /// </summary>
    public IReadOnlyList<BulkCreateProductItemDto>? Products { get; init; }

    /// <summary>
    /// Optional count of synthetic random products to generate and insert (e.g. 100,000).
    /// When specified, populates the catalog with randomized pricing, stock, and assigns to 'SERVIDORES' and 'CLOUD'.
    /// </summary>
    public int? GenerateRandomCount { get; init; }

    /// <summary>
    /// Batch chunk size per database roundtrip (default 1,000).
    /// Tuned to stay within PostgreSQL parameter limits while avoiding Large Object Heap fragmentation.
    /// </summary>
    public int BatchSize { get; init; } = 1000;
}
