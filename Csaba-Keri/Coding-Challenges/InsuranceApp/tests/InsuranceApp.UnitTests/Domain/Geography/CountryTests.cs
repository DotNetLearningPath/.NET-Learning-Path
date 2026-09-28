using InsuranceApp.Domain.Geography;

namespace InsuranceApp.UnitTests.Domain.Geography;

public sealed class CountryTests
{
    [Theory]
    [InlineData("România", "România")]
    [InlineData("  România", "România")]
    [InlineData("România  ", "România")]
    [InlineData("  România  ", "România")]
    public void Constructor_WhenInputIsValid_PreservesIdentifiersAndTrimsName(
        string name,
        string expectedName
    )
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var country = new Country(id, name);

        // Assert
        Assert.Equal(id, country.Id);
        Assert.Equal(expectedName, country.Name);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" \t\r\n ")]
    public void Constructor_WhenNameIsMissing_ThrowsArgumentException(string? name)
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act & Assert
        var exception = Assert.ThrowsAny<ArgumentException>(
            () => new Country(id, name!)
        );

        Assert.Equal("name", exception.ParamName);
    }

    [Fact]
    public void Constructor_WhenIdIsEmpty_ThrowsArgumentException()
    {
        // Arrange
        var id = Guid.Empty;
        var name = "România";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(
            () => new Country(id, name)
        );

        Assert.Equal("id", exception.ParamName);
    }

    [Fact]
    public void Constructor_WhenTrimmedNameHas100Characters_AcceptsName()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = new string('A', 100);

        // Act
        var country = new Country(id, $"  {name}  ");

        // Assert
        Assert.Equal(name, country.Name);
    }

    [Fact]
    public void Constructor_WhenTrimmedNameExceeds100Characters_ThrowsArgumentException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = new string('A', 101);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(
            () => new Country(id, $"  {name}  ")
        );

        Assert.Equal("name", exception.ParamName);
    }
}
