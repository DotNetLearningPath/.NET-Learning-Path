namespace Application.DTO.Common;

public class PagedResultTest
{
    [Theory]
    [InlineData(0, 10, 0)]
    [InlineData(20, 10, 2)]
    [InlineData(21, 10, 3)]
    [InlineData(1, 10, 1)]
    public void TotalPages_Should_Round_Up_To_The_Next_Page(
        int totalCount,
        int pageSize,
        int expectedTotalPages)
    {
        var result = new PagedResult<string>
        {
            PageNumber = 1,
            PageSize = pageSize,
            TotalCount = totalCount
        };

        Assert.Equal(expectedTotalPages, result.TotalPages);
    }

    [Theory]
    [InlineData(1, 3, false, true)]
    [InlineData(2, 3, true, true)]
    [InlineData(3, 3, true, false)]
    public void Page_Navigation_Flags_Should_Reflect_Current_Page(
        int pageNumber,
        int totalPages,
        bool expectedHasPreviousPage,
        bool expectedHasNextPage)
    {
        var result = new PagedResult<string>
        {
            PageNumber = pageNumber,
            PageSize = 10,
            TotalCount = totalPages * 10
        };

        Assert.Equal(expectedHasPreviousPage, result.HasPreviousPage);
        Assert.Equal(expectedHasNextPage, result.HasNextPage);
    }

    [Fact]
    public void Items_Should_Default_To_An_Empty_Collection()
    {
        var result = new PagedResult<string>();

        Assert.NotNull(result.Items);
        Assert.Empty(result.Items);
    }

    [Fact]
    public void Items_Should_Preserve_Provided_Values()
    {
        var items = new[] { "first", "second" };
        var result = new PagedResult<string>
        {
            Items = items,
            PageNumber = 1,
            PageSize = 10,
            TotalCount = items.Length
        };

        Assert.Equal(items, result.Items);
    }
}
