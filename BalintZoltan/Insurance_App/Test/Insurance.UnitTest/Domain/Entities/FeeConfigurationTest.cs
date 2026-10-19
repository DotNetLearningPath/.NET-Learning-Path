using Insurance.Domain.Entities;
using Insurance.Domain.Enums;

namespace Insurance.UnitTest.Domain.Entities;

public sealed class FeeConfigurationTest
{
    private readonly DateTime _effectiveFrom;

    public FeeConfigurationTest()
    {
        _effectiveFrom = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    }

    [Theory]
    [InlineData(FeeType.BrokerCommission)]
    [InlineData(FeeType.RiskAdjustment)]
    [InlineData(FeeType.AdminFee)]
    public void Constructor_CreatesFeeConfigurationWithValidType(FeeType type)
    {
        // Arrange & Act
        var fee = CreateFee(type);

        // Assert
        Assert.NotEqual(Guid.Empty, fee.Id);
        Assert.Equal("Standard fee", fee.Name);
        Assert.Equal(type, fee.Type);
        Assert.Equal(5.5m, fee.Percentage);
        Assert.Equal(_effectiveFrom, fee.EffectiveFrom);
        Assert.Null(fee.EffectiveTo);
        Assert.True(fee.IsActive);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(100)]
    public void Constructor_AcceptsPercentageAtValidBounds(decimal percentage)
    {
        // Arrange & Act
        var fee = new FeeConfiguration(
            "Valid fee",
            FeeType.AdminFee,
            percentage,
            _effectiveFrom);

        // Assert
        Assert.Equal(percentage, fee.Percentage);
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(100.01)]
    public void Constructor_ThrowsWhenPercentageIsOutsideValidRange(decimal percentage)
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new FeeConfiguration(
                "Invalid fee",
                FeeType.AdminFee,
                percentage,
                _effectiveFrom));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_ThrowsWhenNameIsMissing(string? name)
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new FeeConfiguration(
                name!,
                FeeType.AdminFee,
                5m,
                _effectiveFrom));
    }

    [Fact]
    public void Constructor_ThrowsWhenTypeIsUndefined()
    {
        // Arrange
        var invalidType = (FeeType)999;

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new FeeConfiguration(
                "Invalid fee",
                invalidType,
                5m,
                _effectiveFrom));
    }

    [Fact]
    public void Constructor_ThrowsWhenEffectiveEndPrecedesEffectiveStart()
    {
        // Arrange
        var effectiveTo = _effectiveFrom.AddDays(-1);

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new FeeConfiguration(
                "Invalid period",
                FeeType.AdminFee,
                5m,
                _effectiveFrom,
                effectiveTo));
    }

    [Fact]
    public void Update_ChangesConfigurationFields()
    {
        // Arrange
        var fee = CreateFee(FeeType.BrokerCommission);
        var effectiveTo = _effectiveFrom.AddYears(1);

        // Act
        fee.Update(
            "Updated fee",
            FeeType.RiskAdjustment,
            12.25m,
            _effectiveFrom.AddDays(1),
            effectiveTo);

        // Assert
        Assert.Equal("Updated fee", fee.Name);
        Assert.Equal(FeeType.RiskAdjustment, fee.Type);
        Assert.Equal(12.25m, fee.Percentage);
        Assert.Equal(_effectiveFrom.AddDays(1), fee.EffectiveFrom);
        Assert.Equal(effectiveTo, fee.EffectiveTo);
    }

    [Fact]
    public void Update_ThrowsWhenEffectiveEndPrecedesEffectiveStart()
    {
        // Arrange
        var fee = CreateFee(FeeType.BrokerCommission);
        var invalidEffectiveFrom = _effectiveFrom.AddDays(1);
        var invalidEffectiveTo = _effectiveFrom;

        // Act
        var exception = Assert.Throws<ArgumentException>(() =>
            fee.Update(
                "Updated fee",
                FeeType.AdminFee,
                12m,
                invalidEffectiveFrom,
                invalidEffectiveTo));

        // Assert
        Assert.Equal("effectiveTo", exception.ParamName);
        Assert.Equal("Standard fee", fee.Name);
        Assert.Equal(FeeType.BrokerCommission, fee.Type);
        Assert.Equal(5.5m, fee.Percentage);
        Assert.Equal(_effectiveFrom, fee.EffectiveFrom);
        Assert.Null(fee.EffectiveTo);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public void Update_ThrowsWhenPercentageIsOutsideValidRange(decimal percentage)
    {
        // Arrange
        var fee = CreateFee(FeeType.BrokerCommission);

        // Act
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            fee.Update(
                "Updated fee",
                FeeType.AdminFee,
                percentage,
                _effectiveFrom,
                null));

        // Assert
        Assert.Equal(5.5m, fee.Percentage);
        Assert.Equal(FeeType.BrokerCommission, fee.Type);
    }

    [Fact]
    public void DeactivateAndActivate_ChangeActiveState()
    {
        // Arrange
        var fee = CreateFee(FeeType.AdminFee);

        // Act
        fee.Deactivate();

        // Assert
        Assert.False(fee.IsActive);

        // Act
        fee.Activate();

        // Assert
        Assert.True(fee.IsActive);
    }

    private FeeConfiguration CreateFee(FeeType type)
    {
        return new FeeConfiguration(
            "Standard fee",
            type,
            5.5m,
            _effectiveFrom);
    }
}
