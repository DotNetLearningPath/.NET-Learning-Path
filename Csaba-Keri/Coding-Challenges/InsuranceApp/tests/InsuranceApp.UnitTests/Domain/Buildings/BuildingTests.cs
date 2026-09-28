using InsuranceApp.Domain.Buildings;
using System.Globalization;

namespace InsuranceApp.UnitTests.Domain.Buildings;

public sealed class BuildingTests
{
    [Theory]
    [InlineData(BuildingType.Residential)]
    [InlineData(BuildingType.Office)]
    [InlineData(BuildingType.Industrial)]
    public void Constructor_WhenInputIsValid_PreservesIdentifiersAndDetails(BuildingType type)
    {
        // Arrange
        var id = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var address = new BuildingAddress(Guid.NewGuid(), "Original Street", "12A");
        var constructionYear = 2026;
        var numberOfFloors = 3;
        var surfaceArea = 123.45m;
        var insuredValue = 999.99m;

        // Act
        var building = CreateBuilding(
            id: id,
            clientId: clientId,
            type: type,
            address: address,
            constructionYear: constructionYear,
            numberOfFloors: numberOfFloors,
            surfaceArea: surfaceArea,
            insuredValue: insuredValue
        );

        // Assert
        Assert.Equal(id, building.Id);
        Assert.Equal(clientId, building.ClientId);
        Assert.Equal(type, building.Type);
        Assert.Equal(address, building.Address);
        Assert.Equal(constructionYear, building.ConstructionYear);
        Assert.Equal(numberOfFloors, building.NumberOfFloors);
        Assert.Equal(surfaceArea, building.SurfaceArea);
        Assert.Equal(insuredValue, building.InsuredValue);
    }

    [Fact]
    public void Constructor_WhenIdIsEmpty_ThrowsArgumentException()
    {
        // Arrange
        var id = Guid.Empty;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(
            () => CreateBuilding(id: id)
        );

        Assert.Equal("id", exception.ParamName);
    }

    [Fact]
    public void Constructor_WhenClientIdIsEmpty_ThrowsArgumentException()
    {
        // Arrange
        var clientId = Guid.Empty;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(
            () => CreateBuilding(clientId: clientId)
        );

        Assert.Equal("clientId", exception.ParamName);
    }

    [Fact]
    public void Constructor_WhenAddressIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var type = BuildingType.Residential;
        var constructionYear = 2026;
        var numberOfFloors = 3;
        var surfaceArea = 123.45m;
        var insuredValue = 999.99m;

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(
            () => new Building(
                id: id,
                clientId: clientId,
                type: type,
                address: null!,
                constructionYear: constructionYear,
                numberOfFloors: numberOfFloors,
                surfaceArea: surfaceArea,
                insuredValue: insuredValue
            )
        );

        Assert.Equal("address", exception.ParamName);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(10000)]
    public void Constructor_WhenConstructionYearIsOutsideCalendarRange_ThrowsArgumentOutOfRangeException(int constructionYear)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => CreateBuilding(constructionYear: constructionYear)
        );

        Assert.Equal("constructionYear", exception.ParamName);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(9999)]
    public void Constructor_WhenConstructionYearIsAtCalendarBoundary_AcceptsValue(int constructionYear)
    {
        // Act
        var building = CreateBuilding(constructionYear: constructionYear);

        // Assert
        Assert.Equal(constructionYear, building.ConstructionYear);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(999)]
    public void Constructor_WhenTypeIsUndefined_ThrowsArgumentOutOfRangeException(int value)
    {
        // Arrange
        var type = (BuildingType)value;

        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => CreateBuilding(type: type)
        );

        Assert.Equal("type", exception.ParamName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_WhenNumberOfFloorsIsNotPositive_ThrowsArgumentOutOfRangeException(int numberOfFloors)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => CreateBuilding(numberOfFloors: numberOfFloors)
        );

        Assert.Equal("numberOfFloors", exception.ParamName);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-0.01")]
    [InlineData("10000000000")]
    public void Constructor_WhenSurfaceAreaIsOutsideRange_ThrowsArgumentOutOfRangeException(string input)
    {
        // Arrange
        var value = decimal.Parse(input, CultureInfo.InvariantCulture);

        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => CreateBuilding(surfaceArea: value)
        );

        Assert.Equal("surfaceArea", exception.ParamName);
    }

    [Theory]
    [InlineData("0.01")]
    [InlineData("9999999999.99")]
    public void Constructor_WhenSurfaceAreaIsAtAcceptedBoundary_AcceptsValue(string input)
    {
        // Arrange
        var value = decimal.Parse(input, CultureInfo.InvariantCulture);

        // Act
        var building = CreateBuilding(surfaceArea: value);

        // Assert
        Assert.Equal(value, building.SurfaceArea);
    }

    [Theory]
    [InlineData("1.001")]
    [InlineData("0.001")]
    public void Constructor_WhenSurfaceAreaRequiresMoreThanTwoDecimalPlaces_ThrowsArgumentException(string input)
    {
        // Arrange
        var value = decimal.Parse(input, CultureInfo.InvariantCulture);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(
            () => CreateBuilding(surfaceArea: value)
        );

        Assert.Equal("surfaceArea", exception.ParamName);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-0.01")]
    [InlineData("10000000000000000")]
    public void Constructor_WhenInsuredValueIsOutsideRange_ThrowsArgumentOutOfRangeException(string input)
    {
        // Arrange
        var value = decimal.Parse(input, CultureInfo.InvariantCulture);

        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => CreateBuilding(insuredValue: value)
        );

        Assert.Equal("insuredValue", exception.ParamName);
    }

    [Theory]
    [InlineData("0.01")]
    [InlineData("9999999999999999.99")]
    public void Constructor_WhenInsuredValueIsAtAcceptedBoundary_AcceptsValue(string input)
    {
        // Arrange
        var value = decimal.Parse(input, CultureInfo.InvariantCulture);

        // Act
        var building = CreateBuilding(insuredValue: value);

        // Assert
        Assert.Equal(value, building.InsuredValue);
    }

    [Theory]
    [InlineData("1.001")]
    [InlineData("0.001")]
    public void Constructor_WhenInsuredValueRequiresMoreThanTwoDecimalPlaces_ThrowsArgumentException(string input)
    {
        // Arrange
        var value = decimal.Parse(input, CultureInfo.InvariantCulture);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(
            () => CreateBuilding(insuredValue: value)
        );

        Assert.Equal("insuredValue", exception.ParamName);
    }

    [Fact]
    public void Constructor_WhenDecimalValuesHaveTrailingZeroes_AcceptsValues()
    {
        // Arrange
        var surfaceArea = 125.5000m;
        var insuredValue = 250000.0000m;

        // Act
        var building = CreateBuilding(surfaceArea: surfaceArea, insuredValue: insuredValue);

        // Assert
        Assert.Equal(surfaceArea, building.SurfaceArea);
        Assert.Equal(insuredValue, building.InsuredValue);
    }

    [Fact]
    public void UpdateDetails_WhenInputIsValid_UpdatesDetailsAndPreservesOwnerAndIdentity()
    {
        // Arrange
        var building = CreateBuilding();
        var originalId = building.Id;
        var originalClientId = building.ClientId;

        var updatedType = BuildingType.Office;
        var updatedAddress = new BuildingAddress(Guid.NewGuid(), "Updated Street", "20B");
        var updatedConstructionYear = 2010;
        var updatedNumberOfFloors = 5;
        var updatedSurfaceArea = 200.75m;
        var updatedInsuredValue = 350000.25m;

        // Act
        building.UpdateDetails(
            type: updatedType,
            address: updatedAddress,
            constructionYear: updatedConstructionYear,
            numberOfFloors: updatedNumberOfFloors,
            surfaceArea: updatedSurfaceArea,
            insuredValue: updatedInsuredValue
        );

        // Assert
        Assert.Equal(originalId, building.Id);
        Assert.Equal(originalClientId, building.ClientId);

        Assert.Equal(updatedType, building.Type);
        Assert.Equal(updatedAddress, building.Address);
        Assert.Equal(updatedConstructionYear, building.ConstructionYear);
        Assert.Equal(updatedNumberOfFloors, building.NumberOfFloors);
        Assert.Equal(updatedSurfaceArea, building.SurfaceArea);
        Assert.Equal(updatedInsuredValue, building.InsuredValue);
    }

    [Theory]
    [InlineData("type")]
    [InlineData("address")]
    [InlineData("constructionYear")]
    [InlineData("numberOfFloors")]
    [InlineData("surfaceArea")]
    [InlineData("insuredValue")]
    public void UpdateDetails_WhenAnyFieldIsInvalid_LeavesAllExistingValuesUnchanged(string invalidField)
    {
        // Arrange
        var building = CreateBuilding();
        var originalId = building.Id;
        var originalClientId = building.ClientId;
        var originalType = building.Type;
        var originalAddress = building.Address;
        var originalConstructionYear = building.ConstructionYear;
        var originalNumberOfFloors = building.NumberOfFloors;
        var originalSurfaceArea = building.SurfaceArea;
        var originalInsuredValue = building.InsuredValue;

        var type = invalidField == "type" ? (BuildingType)99999 : BuildingType.Office;

        var address = invalidField == "address"
            ? null
            : new BuildingAddress(Guid.NewGuid(), "Updated Street", "20B");

        int constructionYear = invalidField == "constructionYear" ? 0 : 2010;
        int numberOfFloors = invalidField == "numberOfFloors" ? 0 : 3;
        decimal surfaceArea = invalidField == "surfaceArea" ? 0m : 200.75m;
        decimal insuredValue = invalidField == "insuredValue" ? 0m : 350000.25m;

        // Act & Assert
        var exception = Assert.ThrowsAny<ArgumentException>(
            () => building.UpdateDetails(
                type: type,
                address: address!,
                constructionYear: constructionYear,
                numberOfFloors: numberOfFloors,
                surfaceArea: surfaceArea,
                insuredValue: insuredValue
            )
        );

        Assert.Equal(invalidField, exception.ParamName);

        Assert.Equal(originalId, building.Id);
        Assert.Equal(originalClientId, building.ClientId);
        Assert.Equal(originalType, building.Type);
        Assert.Same(originalAddress, building.Address);
        Assert.Equal(originalConstructionYear, building.ConstructionYear);
        Assert.Equal(originalNumberOfFloors, building.NumberOfFloors);
        Assert.Equal(originalSurfaceArea, building.SurfaceArea);
        Assert.Equal(originalInsuredValue, building.InsuredValue);
    }

    private static Building CreateBuilding(
        Guid? id = null,
        Guid? clientId = null,
        BuildingType type = BuildingType.Residential,
        BuildingAddress? address = null,
        int constructionYear = 2005,
        int numberOfFloors = 2,
        decimal surfaceArea = 125.50m,
        decimal insuredValue = 250000m
    )
    {
        return new(
            id: id ?? Guid.NewGuid(),
            clientId: clientId ?? Guid.NewGuid(),
            type: type,
            address: address ?? new(Guid.NewGuid(), "Original Street", "12A"),
            constructionYear: constructionYear,
            numberOfFloors: numberOfFloors,
            surfaceArea: surfaceArea,
            insuredValue: insuredValue
        );
    }
}
