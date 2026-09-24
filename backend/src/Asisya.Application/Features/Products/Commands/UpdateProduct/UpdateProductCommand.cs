using MediatR;

namespace Asisya.Application.Features.Products.Commands.UpdateProduct;

/// <summary>
/// Command to update an existing catalog product record.
/// Enforces business validation and triggers automatic auditing tracking through EF Core.
/// </summary>
public record UpdateProductCommand : IRequest<bool>
{
    /// <summary>
    /// Unique identifier of the product to update.
    /// </summary>
    public int ProductId { get; init; }

    /// <summary>
    /// Commercial name of the merchandise item.
    /// </summary>
    public string ProductName { get; init; } = string.Empty;

    /// <summary>
    /// Category foreign key identifier.
    /// </summary>
    public int? CategoryId { get; init; }

    /// <summary>
    /// Vendor supplier foreign key identifier.
    /// </summary>
    public int? SupplierId { get; init; }

    /// <summary>
    /// Standard sale or transport packaging unit.
    /// </summary>
    public string? QuantityPerUnit { get; init; }

    /// <summary>
    /// Monetary unit price for the product.
    /// </summary>
    public decimal? UnitPrice { get; init; }

    /// <summary>
    /// Stock quantity currently available in warehouse inventory.
    /// </summary>
    public short? UnitsInStock { get; init; }

    /// <summary>
    /// Stock quantity on backorder from suppliers.
    /// </summary>
    public short? UnitsOnOrder { get; init; }

    /// <summary>
    /// Reorder threshold level.
    /// </summary>
    public short? ReorderLevel { get; init; }

    /// <summary>
    /// Flag indicating whether this product has been retired or discontinued.
    /// </summary>
    public bool Discontinued { get; init; }
}
