using Asisya.Application.Features.Products.Commands.BulkCreateProducts;

namespace Asisya.Application.Features.Products.Events;

/// <summary>
/// Integration event published when a bulk product ingestion job is accepted by the API.
/// Dispatched to RabbitMQ and consumed asynchronously by <see cref="Consumers.BulkCreateProductsConsumer"/>
/// to process up to 100,000 items in high-throughput streaming batches without blocking the HTTP pipeline.
/// </summary>
public record BatchProductsReceivedEvent
{
    /// <summary>
    /// Unique correlation identifier for the batch ingestion process.
    /// </summary>
    public Guid BatchId { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Timestamp when the batch request was received and enqueued in UTC.
    /// </summary>
    public DateTime EnqueuedAtUtc { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Explicit collection of product items to insert, if provided.
    /// </summary>
    public IReadOnlyList<BulkCreateProductItemDto>? Products { get; init; }

    /// <summary>
    /// Optional count of synthetic random products to generate and insert (e.g. 100,000).
    /// </summary>
    public int? GenerateRandomCount { get; init; }

    /// <summary>
    /// Batch chunk size per database transaction roundtrip (default 1,000).
    /// </summary>
    public int BatchSize { get; init; } = 1000;
}
