namespace Asisya.Application.Features.Products.Queries.GetProductById;

/// <summary>
/// Detailed category information embedded in the product detail view, including image data.
/// </summary>
public record CategoryDetailDto
{
    /// <summary>
    /// Unique category identifier.
    /// </summary>
    public int CategoryId { get; init; }

    /// <summary>
    /// Commercial category name.
    /// </summary>
    public string CategoryName { get; init; } = string.Empty;

    /// <summary>
    /// Narrative description of the category.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Raw binary picture data stored in PostgreSQL bytea.
    /// </summary>
    public byte[]? Picture { get; init; }

    /// <summary>
    /// Base64 data URI string formatted for direct rendering in web image tags (e.g. "data:image/jpeg;base64,...").
    /// </summary>
    public string? PictureBase64 => Picture != null && Picture.Length > 0
        ? $"data:image/jpeg;base64,{Convert.ToBase64String(Picture)}"
        : null;
}
