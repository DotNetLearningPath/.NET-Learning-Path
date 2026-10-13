using InsuranceApp.Domain.RiskFactors;
using InsuranceApp.Domain.RiskFactors.Targets;
using InsuranceApp.UnitTests.TestData.RiskFactors;

namespace InsuranceApp.UnitTests.Domain.RiskFactors;

public sealed class RiskFactorConfigurationTests
{
    [Theory]
    [InlineData(RiskFactorLevel.Country)]
    [InlineData(RiskFactorLevel.County)]
    [InlineData(RiskFactorLevel.City)]
    [InlineData(RiskFactorLevel.BuildingType)]
    public void Constructor_ValidTarget_PreservesAllFields(RiskFactorLevel level)
    {
        // Arrange
        var id = Guid.NewGuid();
        var target = RiskTargetTestData.Create(level);
        var adjustmentPercentage = -2.1234m;
        var isActive = false;

        // Act
        var riskFactor = RiskFactorConfigurationTestData.Create(
            id: id,
            target: target,
            adjustmentPercentage: adjustmentPercentage,
            isActive: isActive
        );

        // Assert
        Assert.Equal(id, riskFactor.Id);
        Assert.Equal(target, riskFactor.Target);
        Assert.Equal(level, riskFactor.Target.Level);
        Assert.Equal(adjustmentPercentage, riskFactor.AdjustmentPercentage);
        Assert.False(riskFactor.IsActive);
    }

    [Theory]
    [InlineData(-100)]
    [InlineData(0)]
    [InlineData(100)]
    public void Constructor_ValidAdjustment_AcceptsBoundaries(int adjustment)
    {
        // Act
        var riskFactor = RiskFactorConfigurationTestData.Create(adjustmentPercentage: adjustment);

        // Assert
        Assert.Equal(adjustment, riskFactor.AdjustmentPercentage);
    }

    [Theory]
    [InlineData("-100.0001")]
    [InlineData("100.0001")]
    [InlineData("1.12345")]
    public void Constructor_InvalidAdjustment_Throws(string value)
    {
        // Arrange
        var adjustment = decimal.Parse(value, System.Globalization.CultureInfo.InvariantCulture);

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(
            () => RiskFactorConfigurationTestData.Create(adjustmentPercentage: adjustment)
        );
    }

    [Fact]
    public void Constructor_NullTarget_Throws()
    {
        // Arrange
        RiskTarget target = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(
            () => new RiskFactorConfiguration(
                id: RiskFactorConfigurationTestData.DefaultId,
                target: target,
                adjustmentPercentage: RiskFactorConfigurationTestData.DefaultAdjustmentPercentage,
                isActive: RiskFactorConfigurationTestData.DefaultIsActive
            )
        );
    }

    [Fact]
    public void Constructor_EmptyId_Throws()
    {
        // Arrange
        var id = Guid.Empty;

        // Act & Assert
        Assert.Throws<ArgumentException>(
            () => RiskFactorConfigurationTestData.Create(id: id)
        );
    }

    [Fact]
    public void UpdateDetails_ValidTargetChange_ClearsPreviousTargetAndPreservesId()
    {
        // Arrange
        var riskFactorId = Guid.NewGuid();
        var oldTarget = RiskTargetTestData.CreateCityTarget();

        var riskFactor = RiskFactorConfigurationTestData.Create(
            id: riskFactorId,
            target: oldTarget
        );

        var newTarget = RiskTargetTestData.CreateBuildingTypeTarget();
        var newAdjustmentPercentage = -5m;
        var newIsActive = false;

        // Act
        riskFactor.UpdateDetails(
            target: newTarget,
            adjustmentPercentage: newAdjustmentPercentage,
            isActive: newIsActive
        );

        // Assert
        Assert.Equal(riskFactorId, riskFactor.Id);
        Assert.Equal(newTarget.Level, riskFactor.Target.Level);
        Assert.Equal(newTarget, riskFactor.Target);
        Assert.Equal(newAdjustmentPercentage, riskFactor.AdjustmentPercentage);
        Assert.False(riskFactor.IsActive);
    }

    [Fact]
    public void UpdateDetails_InvalidAdjustment_DoesNotPartiallyMutate()
    {
        // Arrange
        var riskFactor = RiskFactorConfigurationTestData.Create();
        var invalidAdjustment = 101m;

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(
            () => riskFactor.UpdateDetails(
                target: RiskTargetTestData.CreateBuildingTypeTarget(),
                adjustmentPercentage: invalidAdjustment,
                isActive: false
            )
        );

        Assert.Equal(RiskFactorConfigurationTestData.DefaultRiskTarget.Level, riskFactor.Target.Level);
        Assert.Equal(RiskFactorConfigurationTestData.DefaultRiskTarget, riskFactor.Target);
        Assert.Equal(RiskFactorConfigurationTestData.DefaultAdjustmentPercentage, riskFactor.AdjustmentPercentage);
        Assert.Equal(RiskFactorConfigurationTestData.DefaultIsActive, riskFactor.IsActive);
    }

    [Fact]
    public void UpdateDetails_NullTarget_DoesNotPartiallyMutate()
    {
        // Arrange
        var riskFactor = RiskFactorConfigurationTestData.Create();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(
            () => riskFactor.UpdateDetails(
                target: null!,
                adjustmentPercentage: -5m,
                isActive: false
            )
        );

        Assert.Equal(RiskFactorConfigurationTestData.DefaultRiskTarget, riskFactor.Target);
        Assert.Equal(RiskFactorConfigurationTestData.DefaultAdjustmentPercentage, riskFactor.AdjustmentPercentage);
        Assert.Equal(RiskFactorConfigurationTestData.DefaultIsActive, riskFactor.IsActive);
    }
}
