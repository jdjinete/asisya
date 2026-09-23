namespace Asisya.Domain.Entities;

/// <summary>
/// Represents a product category within the ASISYA enterprise catalog.
/// Used to classify products and store category presentation media.
/// </summary>
public class Category
{
    /// <summary>
    /// Unique identifier for the Category (Primary Key).
    /// </summary>
    public int CategoryId { get; set; }

    /// <summary>
    /// Commercial or departmental name of the category (e.g., Beverages, Condiments).
    /// </summary>
    public string CategoryName { get; set; } = string.Empty;

    /// <summary>
    /// Narrative description explaining the types of products grouped under this category.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Binary image data or legacy OLE picture representation of the category.
    /// In ASISYA, this can be served via streaming or base64 representation.
    /// </summary>
    public byte[]? Picture { get; set; }

    /// <summary>
    /// Navigation property referencing the collection of products belonging to this category.
    /// Enforces one-to-many relationship with referential integrity.
    /// </summary>
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
