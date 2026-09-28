using InsuranceApp.Application.Common.Pagination;

namespace InsuranceApp.UnitTests.Application.Common.Pagination;

public sealed class PagedResultTests
{
    [Fact]
    public void Constructor_WhenSourceCollectionChanges_KeepsOriginalPageItems()
    {
        // Arrange
        var items = new List<int> { 10, 20 };
        var itemsCopy = new List<int>(items);

        var pageNumber = 2;
        var pageSize = 2;
        var totalCount = 47L;

        // Act
        var page = new PagedResult<int>(items, pageNumber, pageSize, totalCount);
        items.Clear();

        // Assert
        Assert.Equal(itemsCopy, page.Items);
        Assert.Equal(pageNumber, page.PageNumber);
        Assert.Equal(pageSize, page.PageSize);
        Assert.Equal(totalCount, page.TotalCount);
    }

    [Fact]
    public void Constructor_WhenItemsAreNull_ThrowsArgumentNullException()
    {
        // Arrange
        var pageNumber = 2;
        var pageSize = 2;
        var totalCount = 47L;

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(
            () => new PagedResult<int>(null!, pageNumber, pageSize, totalCount)
        );

        Assert.Equal("items", exception.ParamName);
    }

    [Theory]
    [InlineData(0, 20, 0L, "pageNumber")]
    [InlineData(1, 0, 0L, "pageSize")]
    [InlineData(1, 101, 0L, "pageSize")]
    [InlineData(1, 20, -1L, "totalCount")]
    public void Constructor_WhenPaginationMetadataIsInvalid_ThrowsArgumentOutOfRangeException(
        int pageNumber, int pageSize, long totalCount, string invalidParameter
    )
    {
        // Arrange
        var items = Array.Empty<int>();

        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => new PagedResult<int>(items, pageNumber, pageSize, totalCount)
        );

        Assert.Equal(invalidParameter, exception.ParamName);
    }

    [Fact]
    public void Constructor_WhenPageIsEmpty_PreservesTotalCount()
    {
        // Arrange
        var items = Array.Empty<int>();
        var pageNumber = 100;
        var pageSize = 20;
        var totalCount = 5L;

        // Act
        var page = new PagedResult<int>(items, pageNumber, pageSize, totalCount);

        // Assert
        Assert.Empty(page.Items);
        Assert.Equal(totalCount, page.TotalCount);
    }
}
