namespace Asisya.Domain.Entities;

/// <summary>
/// Represents a commercial merchandise item cataloged within ASISYA.
/// Maintains inventory status, pricing, and associations with categories and suppliers.
/// </summary>
public class Product
{
    /// <summary>
    /// Unique identifier for the Product (Primary Key).
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// Commercial display name of the product.
    /// Indexed for rapid textual and full-text searches.
    /// </summary>
    public string ProductName { get; set; } = string.Empty;

    /// <summary>
    /// Foreign key referencing the Supplier that provides this product.
    /// Nullable when supplier information is unassigned or external.
    /// </summary>
    public int? SupplierId { get; set; }

    /// <summary>
    /// Foreign key referencing the Category to which this product belongs.
    /// Critical relational link ensuring categorized filtering and catalog grouping.
    /// </summary>
    public int? CategoryId { get; set; }

    /// <summary>
    /// Packaging specification or sales packaging unit (e.g., "12 - 12 oz bottles").
    /// </summary>
    public string? QuantityPerUnit { get; set; }

    /// <summary>
    /// Unit monetary price for consumer or retail purchase.
    /// </summary>
    public decimal? UnitPrice { get; set; }

    /// <summary>
    /// Quantity of inventory units currently available on-hand in warehouse storage.
    /// </summary>
    public short? UnitsInStock { get; set; }

    /// <summary>
    /// Quantity of inventory units currently on backorder from suppliers.
    /// </summary>
    public short? UnitsOnOrder { get; set; }

    /// <summary>
    /// Minimum threshold level of stock triggering automated warehouse reordering.
    /// </summary>
    public short? ReorderLevel { get; set; }

    /// <summary>
    /// Flag indicating whether this product has been retired or discontinued.
    /// Discontinued items are excluded from standard active catalog searches.
    /// </summary>
    public bool Discontinued { get; set; }

    /// <summary>
    /// Navigation property to the associated Category entity.
    /// Enables eager/lazy loading of category attributes and category image.
    /// </summary>
    public Category? Category { get; set; }

    /// <summary>
    /// Navigation property to the supplying vendor.
    /// </summary>
    public Supplier? Supplier { get; set; }

    /// <summary>
    /// Navigation collection of order line items referencing this product.
    /// </summary>
    public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}
