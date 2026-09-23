using System.Diagnostics;
using Asisya.Application.Common.Interfaces;
using Asisya.Application.Features.Products.Events;
using Asisya.Domain.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Asisya.Application.Features.Products.Consumers;

/// <summary>
/// MassTransit background worker consumer that processes <see cref="BatchProductsReceivedEvent"/>
/// from the RabbitMQ message queue.
/// 
/// High-Throughput Batch Processing Architecture:
/// 1. Asynchronous Decoupling: Offloads execution from the HTTP thread pool, returning HTTP 202 Accepted
///    instantly while processing large workloads (e.g. 100,000 items) in the background.
/// 2. Transactional Chunking: Groups records into chunks (default 1,000 items) executed within
///    <see cref="IApplicationDbContext.ExecuteInTransactionAsync"/> compatible with PostgreSQL execution strategies.
/// 3. Memory Eviction (ChangeTracker.Clear): Clears EF Core's ChangeTracker after each chunk flush to prevent
///    O(N^2) state inspection overhead and memory leaks across hundreds of thousands of entities.
/// 4. Referential Integrity: Automatically resolves or seeds required enterprise categories ('SERVIDORES', 'CLOUD').
/// </summary>
public class BulkCreateProductsConsumer : IConsumer<BatchProductsReceivedEvent>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<BulkCreateProductsConsumer> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="BulkCreateProductsConsumer"/> class.
    /// </summary>
    public BulkCreateProductsConsumer(
        IApplicationDbContext context,
        ILogger<BulkCreateProductsConsumer> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task Consume(ConsumeContext<BatchProductsReceivedEvent> context)
    {
        var message = context.Message;
        var cancellationToken = context.CancellationToken;
        var stopwatch = Stopwatch.StartNew();

        _logger.LogInformation(
            "Worker started consuming batch {BatchId} (enqueued at {EnqueuedAtUtc:u}). Synthetic: {Count}, Explicit: {Explicit}",
            message.BatchId, message.EnqueuedAtUtc, message.GenerateRandomCount, message.Products?.Count);

        var (servidoresId, cloudId) = await EnsureDefaultCategoriesExistAsync(cancellationToken);

        int totalToProcess = 0;
        int successfulImports = 0;
        int failedImports = 0;
        var errors = new List<string>();

        var batchSize = message.BatchSize > 0 ? message.BatchSize : 1000;

        var (successCount, failCount) = await _context.ExecuteInTransactionAsync(async ct =>
        {
            int batchSuccess = 0;
            int batchFail = 0;

            if (message.GenerateRandomCount.HasValue && message.GenerateRandomCount.Value > 0)
            {
                totalToProcess = message.GenerateRandomCount.Value;
                var random = new Random(42);
                var serverPrefixes = new[] { "Dell PowerEdge", "HP ProLiant", "Lenovo ThinkSystem", "Cisco UCS", "Supermicro" };
                var cloudPrefixes = new[] { "AWS EC2 Instance", "Azure VM Standard", "GCP Compute Engine", "Kubernetes Node Pod", "Cloud Dedicated Host" };

                var currentChunk = new List<Product>(batchSize);

                for (int i = 1; i <= totalToProcess; i++)
                {
                    var isCloud = (i % 2 == 0);
                    var categoryId = isCloud ? cloudId : servidoresId;
                    var prefix = isCloud
                        ? cloudPrefixes[random.Next(cloudPrefixes.Length)]
                        : serverPrefixes[random.Next(serverPrefixes.Length)];

                    var product = new Product
                    {
                        ProductName = $"{prefix} - Gen{random.Next(10, 16)} #{i:D6}",
                        CategoryId = categoryId,
                        UnitPrice = Math.Round((decimal)(random.NextDouble() * 5000 + 100), 2),
                        UnitsInStock = (short)random.Next(0, 500),
                        UnitsOnOrder = (short)random.Next(0, 50),
                        ReorderLevel = 10,
                        Discontinued = (random.Next(0, 100) < 5),
                        QuantityPerUnit = isCloud ? "1 vCPU / 4GB RAM hourly" : "Rack 1U chassis"
                    };

                    currentChunk.Add(product);

                    if (currentChunk.Count >= batchSize)
                    {
                        await _context.Products.AddRangeAsync(currentChunk, ct);
                        await _context.SaveChangesAsync(ct);
                        _context.ClearChangeTracker();
                        batchSuccess += currentChunk.Count;
                        currentChunk.Clear();
                    }
                }

                if (currentChunk.Count > 0)
                {
                    await _context.Products.AddRangeAsync(currentChunk, ct);
                    await _context.SaveChangesAsync(ct);
                    _context.ClearChangeTracker();
                    batchSuccess += currentChunk.Count;
                    currentChunk.Clear();
                }
            }
            else if (message.Products != null && message.Products.Count > 0)
            {
                totalToProcess = message.Products.Count;
                var currentChunk = new List<Product>(batchSize);

                foreach (var item in message.Products)
                {
                    if (string.IsNullOrWhiteSpace(item.ProductName))
                    {
                        batchFail++;
                        errors.Add("Encountered product item with empty or null ProductName.");
                        continue;
                    }

                    var product = new Product
                    {
                        ProductName = item.ProductName.Trim(),
                        CategoryId = item.CategoryId ?? servidoresId,
                        SupplierId = item.SupplierId,
                        UnitPrice = item.UnitPrice,
                        UnitsInStock = item.UnitsInStock ?? 0,
                        Discontinued = item.Discontinued,
                        QuantityPerUnit = item.QuantityPerUnit
                    };

                    currentChunk.Add(product);

                    if (currentChunk.Count >= batchSize)
                    {
                        await _context.Products.AddRangeAsync(currentChunk, ct);
                        await _context.SaveChangesAsync(ct);
                        _context.ClearChangeTracker();
                        batchSuccess += currentChunk.Count;
                        currentChunk.Clear();
                    }
                }

                if (currentChunk.Count > 0)
                {
                    await _context.Products.AddRangeAsync(currentChunk, ct);
                    await _context.SaveChangesAsync(ct);
                    _context.ClearChangeTracker();
                    batchSuccess += currentChunk.Count;
                    currentChunk.Clear();
                }
            }

            return (batchSuccess, batchFail);
        }, cancellationToken);

        successfulImports = successCount;
        failedImports = failCount;
        stopwatch.Stop();

        _logger.LogInformation(
            "Worker finished processing batch {BatchId}. Total: {Total}, Committed: {Committed}, Failed: {Failed} in {ElapsedMs} ms",
            message.BatchId, totalToProcess, successfulImports, failedImports, stopwatch.ElapsedMilliseconds);
    }

    private async Task<(int ServidoresId, int CloudId)> EnsureDefaultCategoriesExistAsync(CancellationToken cancellationToken)
    {
        var servidores = await _context.Categories
            .FirstOrDefaultAsync(c => c.CategoryName == "SERVIDORES", cancellationToken);

        if (servidores == null)
        {
            servidores = new Category
            {
                CategoryName = "SERVIDORES",
                Description = "Dedicated on-premises enterprise rack and blade server units"
            };
            _context.Categories.Add(servidores);
        }

        var cloud = await _context.Categories
            .FirstOrDefaultAsync(c => c.CategoryName == "CLOUD", cancellationToken);

        if (cloud == null)
        {
            cloud = new Category
            {
                CategoryName = "CLOUD",
                Description = "Scalable cloud instances, virtual machines, and managed cloud infrastructure"
            };
            _context.Categories.Add(cloud);
        }

        await _context.SaveChangesAsync(cancellationToken);
        _context.ClearChangeTracker();

        return (servidores.CategoryId, cloud.CategoryId);
    }
}
