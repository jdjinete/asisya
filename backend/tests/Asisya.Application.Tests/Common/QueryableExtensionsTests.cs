using Asisya.Application.Common.Extensions;
using Asisya.Application.Tests.Common;
using Asisya.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Asisya.Application.Tests.Common;

public class QueryableExtensionsTests : IDisposable
{
    private readonly TestAsisyaDbContext _context;

    public QueryableExtensionsTests()
    {
        _context = TestDbContextFactory.Create();
    }

    [Fact]
    public async Task ToPaginatedListAsync_ShouldReturnPaginatedList_WithCorrectMetadata()
    {
        // Arrange
        for (int i = 1; i <= 25; i++)
        {
            _context.Categories.Add(new Category
            {
                CategoryId = i,
                CategoryName = $"Category {i:D2}",
                Description = $"Description for category {i}"
            });
        }
        await _context.SaveChangesAsync();

        // Act - Request Page 2 with PageSize 10
        var paginated = await _context.Categories
            .OrderBy(c => c.CategoryId)
            .ToPaginatedListAsync(pageIndex: 2, pageSize: 10, CancellationToken.None);

        // Assert
        paginated.Should().NotBeNull();
        paginated.TotalItems.Should().Be(25);
        paginated.TotalPages.Should().Be(3);
        paginated.PageIndex.Should().Be(2);
        paginated.PageSize.Should().Be(10);
        paginated.HasPreviousPage.Should().BeTrue();
        paginated.HasNextPage.Should().BeTrue();
        paginated.Items.Should().HaveCount(10);
        paginated.Items[0].CategoryId.Should().Be(11);
        paginated.Items[9].CategoryId.Should().Be(20);
    }

    public void Dispose()
    {
        TestDbContextFactory.Destroy(_context);
    }
}
