using InsuranceApp.Domain.Geography;

namespace InsuranceApp.UnitTests.Domain.Geography;

public sealed class CityTests
{
    [Theory]
    [InlineData("Cluj-Napoca", "Cluj-Napoca")]
    [InlineData("  Cluj-Napoca", "Cluj-Napoca")]
    [InlineData("Cluj-Napoca  ", "Cluj-Napoca")]
    [InlineData("  Cluj-Napoca  ", "Cluj-Napoca")]
    public void Constructor_WhenInputIsValid_PreservesIdentifiersAndTrimsName(
        string name,
        string expectedName
    )
    {
        // Arrange
        var id = Guid.NewGuid();
        var countyId = Guid.NewGuid();

        // Act
        var city = new City(id, name, countyId);

        // Assert
        Assert.Equal(id, city.Id);
        Assert.Equal(expectedName, city.Name);
        Assert.Equal(countyId, city.CountyId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" \t\r\n ")]
    public void Constructor_WhenNameIsMissing_ThrowsArgumentException(string? name)
    {
        // Arrange
        var id = Guid.NewGuid();
        var countyId = Guid.NewGuid();

        // Act & Assert
        var exception = Assert.ThrowsAny<ArgumentException>(
            () => new City(id, name!, countyId)
        );

        Assert.Equal("name", exception.ParamName);
    }

    [Fact]
    public void Constructor_WhenIdIsEmpty_ThrowsArgumentException()
    {
        // Arrange
        var id = Guid.Empty;
        var name = "Cluj-Napoca";
        var countyId = Guid.NewGuid();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(
            () => new City(id, name, countyId)
        );

        Assert.Equal("id", exception.ParamName);
    }

    [Fact]
    public void Constructor_WhenTrimmedNameHas100Characters_AcceptsName()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = new string('A', 100);
        var countyId = Guid.NewGuid();

        // Act
        var city = new City(id, $"  {name}  ", countyId);

        // Assert
        Assert.Equal(name, city.Name);
    }

    [Fact]
    public void Constructor_WhenTrimmedNameExceeds100Characters_ThrowsArgumentException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = new string('A', 101);
        var countyId = Guid.NewGuid();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(
            () => new City(id, $"  {name}  ", countyId)
        );

        Assert.Equal("name", exception.ParamName);
    }

    [Fact]
    public void Constructor_WhenCountyIdIsEmpty_ThrowsArgumentException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = "Cluj-Napoca";
        var countyId = Guid.Empty;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(
            () => new City(id, name, countyId)
        );

        Assert.Equal("countyId", exception.ParamName);
    }
}
