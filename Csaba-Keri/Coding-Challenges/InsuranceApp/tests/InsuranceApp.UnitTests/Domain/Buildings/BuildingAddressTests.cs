using InsuranceApp.Domain.Buildings;

namespace InsuranceApp.UnitTests.Domain.Buildings;

public sealed class BuildingAddressTests
{
    [Theory]
    [InlineData("Example Street", "12A", "12A")]
    [InlineData("  Example Street  ", "  10-12  ", "10-12")]
    public void Constructor_WhenInputIsValid_NormalizesTextAndPreservesCity(
        string street,
        string number,
        string expectedNumber
    )
    {
        // Arrange
        var cityId = Guid.NewGuid();

        // Act
        var address = new BuildingAddress(cityId, street, number);

        // Assert
        Assert.Equal(cityId, address.CityId);
        Assert.Equal("Example Street", address.Street);
        Assert.Equal(expectedNumber, address.Number);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" \t\r\n ")]
    public void Constructor_WhenStreetIsMissing_ThrowsArgumentException(string? street)
    {
        // Arrange
        var cityId = Guid.NewGuid();
        var number = "12A";

        // Act & Assert
        var exception = Assert.ThrowsAny<ArgumentException>(
            () => new BuildingAddress(cityId, street!, number)
        );

        Assert.Equal("street", exception.ParamName);
    }

    [Fact]
    public void Constructor_WhenStreetIsAtLengthLimit_AcceptsValue()
    {
        // Arrange
        var cityId = Guid.NewGuid();
        var street = new string('A', 200);
        var number = "12A";

        // Act
        var address = new BuildingAddress(cityId, $"  {street}  ", number);

        // Assert
        Assert.Equal(street, address.Street);
    }

    [Fact]
    public void Constructor_WhenStreetIsAboveLengthLimit_ThrowsArgumentException()
    {
        // Arrange
        var cityId = Guid.NewGuid();
        var street = new string('A', 201);
        var number = "12A";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(
            () => new BuildingAddress(cityId, $"  {street}  ", number)
        );

        Assert.Equal("street", exception.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" \t\r\n ")]
    public void Constructor_WhenNumberIsMissing_ThrowsArgumentException(string? number)
    {
        // Arrange
        var cityId = Guid.NewGuid();
        var street = "Example Street";

        // Act & Assert
        var exception = Assert.ThrowsAny<ArgumentException>(
            () => new BuildingAddress(cityId, street, number!)
        );

        Assert.Equal("number", exception.ParamName);
    }

    [Fact]
    public void Constructor_WhenNumberIsAtLengthLimit_AcceptsValue()
    {
        // Arrange
        var cityId = Guid.NewGuid();
        var street = "Example Street";
        var number = new string('A', 20);

        // Act
        var address = new BuildingAddress(cityId, street, $"  {number}  ");

        // Assert
        Assert.Equal(number, address.Number);
    }

    [Fact]
    public void Constructor_WhenNumberIsAboveLengthLimit_ThrowsArgumentException()
    {
        // Arrange
        var cityId = Guid.NewGuid();
        var street = "Example Street";
        var number = new string('A', 21);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(
            () => new BuildingAddress(cityId, street, $"  {number}  ")
        );

        Assert.Equal("number", exception.ParamName);
    }

    [Fact]
    public void Constructor_WhenCityIdIsEmpty_ThrowsArgumentException()
    {
        // Arrange
        var cityId = Guid.Empty;
        var street = "Example Street";
        var number = "12A";

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(
            () => new BuildingAddress(cityId, street, number)
        );

        Assert.Equal("cityId", exception.ParamName);
    }
}
