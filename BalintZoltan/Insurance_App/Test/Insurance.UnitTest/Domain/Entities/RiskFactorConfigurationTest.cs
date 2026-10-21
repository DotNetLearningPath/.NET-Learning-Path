using Insurance.Domain.Entities;
using Insurance.Domain.Enums;

namespace Insurance.UnitTest.Domain.Entities;

public sealed class RiskFactorConfigurationTest
{
    [Fact]
    public void Constructor_ThrowsWhenLevelIsUndefined()
    {
        // Arrange
        var invalidLevel = (RiskFactorLevel)999;

        // Act
        var exception = Assert.Throws<ArgumentException>(() =>
            new RiskFactorConfiguration(invalidLevel, "reference", 5m));

        // Assert
        Assert.Equal("level", exception.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_ThrowsWhenReferenceIsMissing(string? reference)
    {
        // Arrange
        var level = RiskFactorLevel.City;

        // Act
        var exception = Assert.Throws<ArgumentException>(() =>
            new RiskFactorConfiguration(level, reference!, 5m));

        // Assert
        Assert.Equal("reference", exception.ParamName);
    }

    [Fact]
    public void Update_ThrowsWhenLevelIsUndefinedAndDoesNotChangeConfiguration()
    {
        // Arrange
        var configuration = CreateConfiguration();
        var invalidLevel = (RiskFactorLevel)999;

        // Act
        var exception = Assert.Throws<ArgumentException>(() =>
            configuration.Update(invalidLevel, "new-reference", 10m));

        // Assert
        Assert.Equal("level", exception.ParamName);
        Assert.Equal(RiskFactorLevel.City, configuration.Level);
        Assert.Equal("original-reference", configuration.Reference);
        Assert.Equal(5m, configuration.AdjustmentPercentage);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_ThrowsWhenReferenceIsMissingAndDoesNotChangeConfiguration(string? reference)
    {
        // Arrange
        var configuration = CreateConfiguration();

        // Act
        var exception = Assert.Throws<ArgumentException>(() =>
            configuration.Update(RiskFactorLevel.County, reference!, 10m));

        // Assert
        Assert.Equal("reference", exception.ParamName);
        Assert.Equal(RiskFactorLevel.City, configuration.Level);
        Assert.Equal("original-reference", configuration.Reference);
        Assert.Equal(5m, configuration.AdjustmentPercentage);
    }

    private static RiskFactorConfiguration CreateConfiguration() =>
        new(RiskFactorLevel.City, "original-reference", 5m);
}
