namespace Asisya.Domain.Entities;

/// <summary>
/// Represents a freight logistics carrier or postal delivery service responsible for order shipping.
/// </summary>
public class Shipper
{
    /// <summary>
    /// Unique identifier for the Shipper (Primary Key).
    /// </summary>
    public int ShipperId { get; set; }

    /// <summary>
    /// Registered business or company name of the logistics transporter.
    /// </summary>
    public string CompanyName { get; set; } = string.Empty;

    /// <summary>
    /// Customer support or dispatch contact telephone number.
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// Navigation property referencing the shipments carried by this logistics partner.
    /// </summary>
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
