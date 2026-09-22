namespace Asisya.Domain.Entities;

/// <summary>
/// Represents a commercial client or customer account placing orders in ASISYA.
/// </summary>
public class Customer
{
    /// <summary>
    /// Unique 5-character string identifier for the Customer (Primary Key).
    /// Inherited from the standard enterprise Northwind alphanumeric key standard.
    /// </summary>
    public string CustomerId { get; set; } = string.Empty;

    /// <summary>
    /// Registered legal corporate or business name of the client.
    /// </summary>
    public string CompanyName { get; set; } = string.Empty;

    /// <summary>
    /// Primary operational contact person.
    /// </summary>
    public string? ContactName { get; set; }

    /// <summary>
    /// Position or job title of the contact individual.
    /// </summary>
    public string? ContactTitle { get; set; }

    /// <summary>
    /// Street address for billing and communications.
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Municipality or city of the customer headquarters.
    /// </summary>
    public string? City { get; set; }

    /// <summary>
    /// State, province, or geographic territory.
    /// </summary>
    public string? Region { get; set; }

    /// <summary>
    /// Postal code or ZIP designation.
    /// </summary>
    public string? PostalCode { get; set; }

    /// <summary>
    /// Country where customer is registered.
    /// </summary>
    public string? Country { get; set; }

    /// <summary>
    /// Primary contact telephone number.
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// Facsimile transmission number.
    /// </summary>
    public string? Fax { get; set; }

    /// <summary>
    /// Navigation property referencing all commercial orders placed by this customer.
    /// </summary>
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
