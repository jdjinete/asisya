namespace Asisya.Domain.Entities;

/// <summary>
/// Represents a commercial vendor or supplier delivering goods to the ASISYA network.
/// </summary>
public class Supplier
{
    /// <summary>
    /// Unique identifier for the Supplier (Primary Key).
    /// </summary>
    public int SupplierId { get; set; }

    /// <summary>
    /// Legal or commercial name of the supplier company.
    /// </summary>
    public string CompanyName { get; set; } = string.Empty;

    /// <summary>
    /// Primary contact representative's full name.
    /// </summary>
    public string? ContactName { get; set; }

    /// <summary>
    /// Professional title or role of the primary contact (e.g., Sales Representative).
    /// </summary>
    public string? ContactTitle { get; set; }

    /// <summary>
    /// Physical street address of the supplier facility.
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// City of the supplier location.
    /// </summary>
    public string? City { get; set; }

    /// <summary>
    /// State, region, or administrative district.
    /// </summary>
    public string? Region { get; set; }

    /// <summary>
    /// Postal or ZIP code.
    /// </summary>
    public string? PostalCode { get; set; }

    /// <summary>
    /// Country of legal registration.
    /// </summary>
    public string? Country { get; set; }

    /// <summary>
    /// Direct contact telephone number.
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// Facsimile transmission number.
    /// </summary>
    public string? Fax { get; set; }

    /// <summary>
    /// Supplier corporate homepage or online portal URL.
    /// </summary>
    public string? HomePage { get; set; }

    /// <summary>
    /// Navigation property referencing products provided by this supplier.
    /// </summary>
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
