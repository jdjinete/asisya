using Asisya.Application.Features.Products.Events;
using FluentValidation;
using FluentValidation.Results;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Asisya.Application.Features.Products.Commands.BulkCreateProducts;

/// <summary>
/// High-performance MediatR handler for <see cref="BulkCreateProductsCommand"/>.
/// 
/// Asynchronous Event-Driven Architecture:
/// 1. Validation: Validates payload parameters before accepting the request.
/// 2. Event Publishing: Dispatches <see cref="BatchProductsReceivedEvent"/> to RabbitMQ via <see cref="IPublishEndpoint"/>.
/// 3. Non-Blocking HTTP Pipeline: Returns HTTP 202 Accepted immediately with correlation <see cref="BulkCreateProductsResult.BatchId"/>
///    allowing callers to monitor progress without holding open long-lived HTTP socket connections.
/// 4. Decoupled Processing: Execution is offloaded to <see cref="Consumers.BulkCreateProductsConsumer"/> running in background.
/// </summary>
public class BulkCreateProductsCommandHandler : IRequestHandler<BulkCreateProductsCommand, BulkCreateProductsResult>
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<BulkCreateProductsCommandHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="BulkCreateProductsCommandHandler"/> class.
    /// </summary>
    public BulkCreateProductsCommandHandler(
        IPublishEndpoint publishEndpoint,
        ILogger<BulkCreateProductsCommandHandler> logger)
    {
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<BulkCreateProductsResult> Handle(BulkCreateProductsCommand request, CancellationToken cancellationToken)
    {
        // 1. Validate request payload
        var hasRandomCount = request.GenerateRandomCount.HasValue && request.GenerateRandomCount.Value > 0;
        var hasProducts = request.Products != null && request.Products.Count > 0;

        if (!hasRandomCount && !hasProducts)
        {
            throw new ValidationException(new[]
            {
                new ValidationFailure("Products", "Either 'GenerateRandomCount' (greater than 0) or 'Products' collection must be provided.")
            });
        }

        var totalToProcess = request.GenerateRandomCount ?? request.Products?.Count ?? 0;
        var batchId = Guid.NewGuid();
        var enqueuedAt = DateTime.UtcNow;

        _logger.LogInformation(
            "Enqueueing bulk ingestion batch {BatchId} to RabbitMQ. Total items: {TotalItems}, BatchSize: {BatchSize}",
            batchId, totalToProcess, request.BatchSize);

        // 2. Publish integration event to RabbitMQ
        var batchEvent = new BatchProductsReceivedEvent
        {
            BatchId = batchId,
            EnqueuedAtUtc = enqueuedAt,
            Products = request.Products,
            GenerateRandomCount = request.GenerateRandomCount,
            BatchSize = request.BatchSize > 0 ? request.BatchSize : 1000
        };

        await _publishEndpoint.Publish(batchEvent, cancellationToken);

        // 3. Return immediate Accepted response
        return new BulkCreateProductsResult
        {
            BatchId = batchId,
            TotalProcessed = totalToProcess,
            SuccessfulImports = 0,
            FailedImports = 0,
            ElapsedMilliseconds = 0,
            Status = "Accepted",
            Message = $"Bulk product ingestion job {batchId} enqueued for asynchronous processing ({totalToProcess:N0} items).",
            EnqueuedAtUtc = enqueuedAt
        };
    }
}
