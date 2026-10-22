using InsuranceApp.Domain.Buildings;
using InsuranceApp.Domain.RiskFactors;
using InsuranceApp.UnitTests.TestData.RiskFactors;

namespace InsuranceApp.UnitTests.Domain.RiskFactors;

public sealed class RiskTargetTests
{
    [Theory]
    [InlineData(RiskFactorLevel.Country)]
    [InlineData(RiskFactorLevel.County)]
    [InlineData(RiskFactorLevel.City)]
    public void Constructor_EmptyGeographicId_Throws(RiskFactorLevel level)
    {
        // Arrange
        var id = Guid.Empty;

        // Act & Assert
        Assert.Throws<ArgumentException>(
            () => RiskTargetTestData.Create(
                level: level,
                id: id
            )
        );
    }

    [Fact]
    public void Constructor_UnknownBuildingType_Throws()
    {
        // Arrange
        var type = (BuildingType)999;

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(
            () => RiskTargetTestData.Create(
                level: RiskFactorLevel.BuildingType,
                buildingType: type
            )
        );
    }

    [Theory]
    [InlineData(RiskFactorLevel.Country)]
    [InlineData(RiskFactorLevel.County)]
    [InlineData(RiskFactorLevel.City)]
    public void Equality_SameGeographicTarget_UsesValueEquality(RiskFactorLevel level)
    {
        // Arrange
        var id = Guid.NewGuid();
        
        var target = RiskTargetTestData.Create(
            level: level,
            id: id
        );

        var equalTarget = RiskTargetTestData.Create(
            level: level,
            id: id
        );
        
        var differentTarget = RiskTargetTestData.Create(
            level: level,
            id: Guid.NewGuid()
        );

        // Act & Assert
        Assert.Equal(target, equalTarget);
        Assert.NotEqual(target, differentTarget);
    }

    [Fact]
    public void Equality_SameIdAtDifferentLevels_IsNotEqual()
    {
        // Arrange
        var id = Guid.NewGuid();

        var countryTarget = RiskTargetTestData.Create(
            level: RiskFactorLevel.Country,
            id: id
        );

        var countyTarget = RiskTargetTestData.Create(
            level: RiskFactorLevel.County,
            id: id
        );

        var cityTarget = RiskTargetTestData.Create(
            level: RiskFactorLevel.City,
            id: id
        );

        // Act & Assert
        Assert.NotEqual(countryTarget, countyTarget);
        Assert.NotEqual(countryTarget, cityTarget);
        Assert.NotEqual(countyTarget, cityTarget);
    }
}
