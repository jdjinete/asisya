namespace Asisya.Domain.Entities;

/// <summary>
/// Represents a commercial purchase transaction placed by a customer in ASISYA.
/// Tracks order lifecycle, freight charges, fulfillment agent, and dispatch logistics.
/// </summary>
public class Order
{
    /// <summary>
    /// Unique identifier for the Order (Primary Key).
    /// </summary>
    public int OrderId { get; set; }

    /// <summary>
    /// Foreign key referencing the Customer who submitted the order.
    /// </summary>
    public string? CustomerId { get; set; }

    /// <summary>
    /// Foreign key referencing the Employee handling or commissioning the order.
    /// </summary>
    public int? EmployeeId { get; set; }

    /// <summary>
    /// Date and timestamp when the transaction was created.
    /// </summary>
    public DateTime? OrderDate { get; set; }

    /// <summary>
    /// Target date by which client requested delivery fulfillment.
    /// </summary>
    public DateTime? RequiredDate { get; set; }

    /// <summary>
    /// Date when consignment was dispatched by the shipping partner.
    /// </summary>
    public DateTime? ShippedDate { get; set; }

    /// <summary>
    /// Foreign key referencing the Shipper delivering the order.
    /// </summary>
    public int? ShipVia { get; set; }

    /// <summary>
    /// Calculated freight and transportation shipping fee.
    /// </summary>
    public decimal? Freight { get; set; }

    /// <summary>
    /// Name of the recipient party at the delivery destination.
    /// </summary>
    public string? ShipName { get; set; }

    /// <summary>
    /// Destination street address for shipment delivery.
    /// </summary>
    public string? ShipAddress { get; set; }

    /// <summary>
    /// Destination city for shipment delivery.
    /// </summary>
    public string? ShipCity { get; set; }

    /// <summary>
    /// Destination region or department.
    /// </summary>
    public string? ShipRegion { get; set; }

    /// <summary>
    /// Destination postal code.
    /// </summary>
    public string? ShipPostalCode { get; set; }

    /// <summary>
    /// Destination country.
    /// </summary>
    public string? ShipCountry { get; set; }

    /// <summary>
    /// Navigation property to the client who placed the order.
    /// </summary>
    public Customer? Customer { get; set; }

    /// <summary>
    /// Navigation property to the internal employee responsible for the order.
    /// </summary>
    public Employee? Employee { get; set; }

    /// <summary>
    /// Navigation property to the assigned logistics shipper.
    /// </summary>
    public Shipper? Shipper { get; set; }

    /// <summary>
    /// Navigation collection of order line details containing individual line items.
    /// </summary>
    public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}
