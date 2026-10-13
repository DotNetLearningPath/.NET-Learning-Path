using InsuranceApp.Domain.Policies;
using InsuranceApp.UnitTests.TestData.Policies;

namespace InsuranceApp.UnitTests.Domain.Policies;

public sealed class AppliedAdjustmentTests
{
    [Theory]
    [MemberData(
        nameof(AppliedAdjustmentTestData.ValidPercentagesBySource),
        MemberType = typeof(AppliedAdjustmentTestData)
    )]
    public void Constructor_ValidPercentage_PreservesValues(AdjustmentSource source, decimal percentage)
    {
        // Arrange
        var configurationId = Guid.NewGuid();

        // Act
        var adjustment = AppliedAdjustmentTestData.Create(
            source: source,
            configurationId: configurationId,
            percentage: percentage
        );

        // Assert
        Assert.Equal(source, adjustment.Source);
        Assert.Equal(configurationId, adjustment.ConfigurationId);
        Assert.Equal(percentage, adjustment.Percentage);
    }

    [Theory]
    [MemberData(
        nameof(AppliedAdjustmentTestData.InvalidPercentagesBySource),
        MemberType = typeof(AppliedAdjustmentTestData)
    )]
    public void Constructor_PercentageOutsideSourceRules_Throws(AdjustmentSource source, decimal percentage)
    {
        // Arrange
        var configurationId = Guid.NewGuid();

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(
            () => AppliedAdjustmentTestData.Create(
                source: source,
                configurationId: configurationId,
                percentage: percentage
            )
        );
    }
}
