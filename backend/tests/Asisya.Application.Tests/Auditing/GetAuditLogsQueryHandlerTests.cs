using Asisya.Application.Features.AuditLogs.Queries.GetAuditLogs;
using Asisya.Application.Tests.Common;
using Asisya.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Asisya.Application.Tests.Auditing;

public class GetAuditLogsQueryHandlerTests : IDisposable
{
    private readonly TestAsisyaDbContext _context;
    private readonly GetAuditLogsQueryHandler _handler;

    public GetAuditLogsQueryHandlerTests()
    {
        _context = TestDbContextFactory.Create();
        _handler = new GetAuditLogsQueryHandler(_context);
    }

    [Fact]
    public async Task Handle_ShouldReturnAuditLogs_InDescendingChronologicalOrder()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var log1 = new AuditLog
        {
            TableName = "Categories",
            Action = "Insert",
            UserId = "user1@asisya.com",
            TimestampUtc = now.AddMinutes(-10),
            NewValues = "{\"CategoryName\":\"Old\"}"
        };
        var log2 = new AuditLog
        {
            TableName = "Products",
            Action = "Update",
            UserId = "user2@asisya.com",
            TimestampUtc = now.AddMinutes(-5),
            OldValues = "{\"Price\":10}",
            NewValues = "{\"Price\":20}"
        };
        var log3 = new AuditLog
        {
            TableName = "Products",
            Action = "Insert",
            UserId = "user3@asisya.com",
            TimestampUtc = now,
            NewValues = "{\"ProductName\":\"Server\"}"
        };

        _context.AuditLogs.AddRange(log1, log2, log3);
        await _context.SaveChangesAsync();

        var query = new GetAuditLogsQuery { PageIndex = 1, PageSize = 10 };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.TotalItems.Should().Be(3);
        result.Items.Should().HaveCount(3);
        result.Items[0].Id.Should().Be(log3.Id);
        result.Items[1].Id.Should().Be(log2.Id);
        result.Items[2].Id.Should().Be(log1.Id);
    }

    [Fact]
    public async Task Handle_ShouldRespectPagination()
    {
        // Arrange
        var now = DateTime.UtcNow;
        for (int i = 1; i <= 5; i++)
        {
            _context.AuditLogs.Add(new AuditLog
            {
                TableName = "Products",
                Action = "Insert",
                UserId = "admin@asisya.com",
                TimestampUtc = now.AddMinutes(i),
                NewValues = $"{{\"Index\":{i}}}"
            });
        }
        await _context.SaveChangesAsync();

        var query = new GetAuditLogsQuery { PageIndex = 2, PageSize = 2 };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.TotalItems.Should().Be(5);
        result.TotalPages.Should().Be(3);
        result.PageIndex.Should().Be(2);
        result.PageSize.Should().Be(2);
        result.Items.Should().HaveCount(2);
        result.HasPreviousPage.Should().BeTrue();
        result.HasNextPage.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldFilterByTableNameAndAction()
    {
        // Arrange
        _context.AuditLogs.AddRange(
            new AuditLog { TableName = "Products", Action = "Insert", TimestampUtc = DateTime.UtcNow.AddMinutes(-3) },
            new AuditLog { TableName = "Products", Action = "Update", TimestampUtc = DateTime.UtcNow.AddMinutes(-2) },
            new AuditLog { TableName = "Categories", Action = "Insert", TimestampUtc = DateTime.UtcNow.AddMinutes(-1) }
        );
        await _context.SaveChangesAsync();

        var query = new GetAuditLogsQuery { TableName = "products", Action = "update" };

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.TotalItems.Should().Be(1);
        result.Items.First().TableName.Should().Be("Products");
        result.Items.First().Action.Should().Be("Update");
    }

    public void Dispose()
    {
        TestDbContextFactory.Destroy(_context);
    }
}
