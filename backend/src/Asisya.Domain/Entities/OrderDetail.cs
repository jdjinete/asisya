namespace Asisya.Domain.Entities;

/// <summary>
/// Represents a specific line item within an Order in ASISYA.
/// Connects products to orders with historical transaction unit pricing, quantity, and discount rate.
/// </summary>
public class OrderDetail
{
    /// <summary>
    /// Foreign key referencing the parent Order (Part of Composite Primary Key).
    /// </summary>
    public int OrderId { get; set; }

    /// <summary>
    /// Foreign key referencing the purchased Product (Part of Composite Primary Key).
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// Actual negotiated or frozen unit price at the time the order was placed.
    /// Preserves financial auditability against future catalog price fluctuations.
    /// </summary>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// Quantity of product units purchased in this order item line.
    /// </summary>
    public short Quantity { get; set; }

    /// <summary>
    /// Discount percentage applied to this line item (0.0 to 1.0).
    /// </summary>
    public float Discount { get; set; }

    /// <summary>
    /// Navigation property to the parent order.
    /// </summary>
    public Order Order { get; set; } = null!;

    /// <summary>
    /// Navigation property to the referenced catalog product.
    /// </summary>
    public Product Product { get; set; } = null!;
}
