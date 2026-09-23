namespace Asisya.Application.Features.Products.Queries.GetProducts;

/// <summary>
/// Lightweight data transfer object returned in paginated catalog search queries.
/// </summary>
public record ProductSummaryDto
{
    /// <summary>
    /// Unique product identifier.
    /// </summary>
    public int ProductId { get; init; }

    /// <summary>
    /// Product commercial display name.
    /// </summary>
    public string ProductName { get; init; } = string.Empty;

    /// <summary>
    /// Foreign key identifier of the category.
    /// </summary>
    public int? CategoryId { get; init; }

    /// <summary>
    /// Denormalized name of the category for rapid UI table presentation.
    /// </summary>
    public string? CategoryName { get; init; }

    /// <summary>
    /// Unit monetary price.
    /// </summary>
    public decimal? UnitPrice { get; init; }

    /// <summary>
    /// Stock units on-hand in warehouse.
    /// </summary>
    public short? UnitsInStock { get; init; }

    /// <summary>
    /// Status indicating whether product is discontinued.
    /// </summary>
    public bool Discontinued { get; init; }

    /// <summary>
    /// Quantity per unit description.
    /// </summary>
    public string? QuantityPerUnit { get; init; }
}
