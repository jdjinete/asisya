namespace Asisya.Application.Features.Products.Queries.GetProductById;

/// <summary>
/// Detailed product response DTO matching GET /Products/{id} specifications.
/// Includes comprehensive product stock metrics, supplier overview, and category picture media.
/// </summary>
public record ProductDetailDto
{
    /// <summary>
    /// Unique product identifier.
    /// </summary>
    public int ProductId { get; init; }

    /// <summary>
    /// Product commercial name.
    /// </summary>
    public string ProductName { get; init; } = string.Empty;

    /// <summary>
    /// Packaging specification string.
    /// </summary>
    public string? QuantityPerUnit { get; init; }

    /// <summary>
    /// Unit monetary price.
    /// </summary>
    public decimal? UnitPrice { get; init; }

    /// <summary>
    /// Current stock on-hand.
    /// </summary>
    public short? UnitsInStock { get; init; }

    /// <summary>
    /// Quantity on backorder.
    /// </summary>
    public short? UnitsOnOrder { get; init; }

    /// <summary>
    /// Reorder warning threshold.
    /// </summary>
    public short? ReorderLevel { get; init; }

    /// <summary>
    /// Flag indicating whether product is discontinued.
    /// </summary>
    public bool Discontinued { get; init; }

    /// <summary>
    /// Detailed category information containing the category picture.
    /// </summary>
    public CategoryDetailDto? Category { get; init; }

    /// <summary>
    /// Identifier of the assigned supplier.
    /// </summary>
    public int? SupplierId { get; init; }

    /// <summary>
    /// Company name of the supplying vendor.
    /// </summary>
    public string? SupplierName { get; init; }
}
