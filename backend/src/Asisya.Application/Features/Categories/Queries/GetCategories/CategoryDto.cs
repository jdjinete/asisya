namespace Asisya.Application.Features.Categories.Queries.GetCategories;

/// <summary>
/// Data transfer object representing category information and associated product metrics.
/// </summary>
public class CategoryDto
{
    /// <summary>
    /// Unique identifier for the category.
    /// </summary>
    public int CategoryId { get; set; }

    /// <summary>
    /// Name of the category.
    /// </summary>
    public string CategoryName { get; set; } = string.Empty;

    /// <summary>
    /// Narrative description of the category.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Base64-encoded string representation of the category picture if available.
    /// </summary>
    public string? PictureBase64 { get; set; }

    /// <summary>
    /// Total count of products currently assigned to this category.
    /// Used by client applications to evaluate referential integrity status.
    /// </summary>
    public int ProductCount { get; set; }
}
