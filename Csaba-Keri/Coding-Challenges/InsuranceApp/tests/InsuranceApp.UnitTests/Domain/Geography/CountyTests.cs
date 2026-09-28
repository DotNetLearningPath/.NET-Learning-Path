using InsuranceApp.Domain.Geography;

namespace InsuranceApp.UnitTests.Domain.Geography;

public sealed class CountyTests
{
    [Theory]
    [InlineData("Cluj", "Cluj")]
    [InlineData("  Cluj", "Cluj")]
    [InlineData("Cluj  ", "Cluj")]
    [InlineData("  Cluj  ", "Cluj")]
    public void Constructor_WhenInputIsValid_PreservesIdentifiersAndTrimsName(
        string name,
        string expectedName
    )
    {
        // Arrange
        var id = Guid.NewGuid();
        var countryId = Guid.NewGuid();

        // Act
        var county = new County(id, name, countryId);

        // Assert
        Assert.Equal(id, county.Id);
        Assert.Equal(expectedName, county.Name);
        Assert.Equal(countryId, county.CountryId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" \t\r\n ")]
    public void Constructor_WhenNameIsMissing_ThrowsArgumentException(string? name)
    {
        // Arrange
        var id = Guid.NewGuid();
        var countryId = Guid.NewGuid();

        // Act & Assert
        var exception = Assert.ThrowsAny<ArgumentException>(
            () => new County(id, name!, countryId)
        );

        Assert.Equal("name", exception.ParamName);
    }

    [Fact]
    public void Constructor_WhenIdIsEmpty_ThrowsArgumentException()
    {
        // Arrange
        var id = Guid.Empty;
        var name = "Cluj";
        var countryId = Guid.NewGuid();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(
            () => new County(id, name, countryId)
        );

        Assert.Equal("id", exception.ParamName);
    }

    [Fact]
    public void Constructor_WhenTrimmedNameHas100Characters_AcceptsName()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = new string('A', 100);
        var countryId = Guid.NewGuid();

        // Act
        var county = new County(id, $"  {name}  ", countryId);

        // Assert
        Assert.Equal(name, county.Name);
    }

    [Fact]
    public void Constructor_WhenTrimmedNameExceeds100Characters_ThrowsArgumentException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = new string('A', 101);
        var countryId = Guid.NewGuid();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(
            () => new County(id, $"  {name}  ", countryId)
        );

        Assert.Equal("name", exception.ParamName);
    }

    [Fact]
    public void Constructor_WhenCountryIdIsEmpty_ThrowsArgumentException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = "Cluj";
        var countryId = Guid.Empty;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(
            () => new County(id, name, countryId)
        );

        Assert.Equal("countryId", exception.ParamName);
    }
}
