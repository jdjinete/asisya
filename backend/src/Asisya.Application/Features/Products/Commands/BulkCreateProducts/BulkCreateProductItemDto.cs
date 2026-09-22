namespace Asisya.Application.Features.Products.Commands.BulkCreateProducts;

/// <summary>
/// Data transfer object representing a single product line item in a bulk creation operation.
/// </summary>
public record BulkCreateProductItemDto
{
    /// <summary>
    /// Name of the product to create.
    /// </summary>
    public string ProductName { get; init; } = string.Empty;

    /// <summary>
    /// Foreign key referencing the product category.
    /// </summary>
    public int? CategoryId { get; init; }

    /// <summary>
    /// Foreign key referencing the supplying vendor.
    /// </summary>
    public int? SupplierId { get; init; }

    /// <summary>
    /// Packaging specification string.
    /// </summary>
    public string? QuantityPerUnit { get; init; }

    /// <summary>
    /// Unit monetary price.
    /// </summary>
    public decimal? UnitPrice { get; init; }

    /// <summary>
    /// Inventory units on-hand.
    /// </summary>
    public short? UnitsInStock { get; init; }

    /// <summary>
    /// Discontinued status.
    /// </summary>
    public bool Discontinued { get; init; }
}
