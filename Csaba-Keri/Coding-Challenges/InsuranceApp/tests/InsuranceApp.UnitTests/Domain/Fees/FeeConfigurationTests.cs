using InsuranceApp.Domain.Fees;

namespace InsuranceApp.UnitTests.Domain.Fees;

public sealed class FeeConfigurationTests
{
    private static readonly DateOnly Start = new(2030, 1, 10);

    [Fact]
    public void Constructor_ValidValues_NormalizesNameAndPreservesFields()
    {
        // Arrange
        var id = Guid.NewGuid();
        var name = "Admin fee";
        var type = FeeType.AdminFee;
        var percentage = 2.125m;
        var isActive = false;

        // Act
        var fee = CreateFeeConfiguration(
            id: id,
            name: $" {name} ",
            type: type,
            percentage: percentage,
            effectiveFrom: Start,
            effectiveTo: null,
            isActive: isActive
        );

        // Assert
        Assert.Equal(id, fee.Id);
        Assert.Equal(name, fee.Name);
        Assert.Equal(type, fee.Type);
        Assert.Equal(percentage, fee.Percentage);
        Assert.Equal(Start, fee.EffectiveFrom);
        Assert.Null(fee.EffectiveTo);
        Assert.False(fee.IsActive);
    }

    [Theory]
    [InlineData("-0.0001")]
    [InlineData("100.0001")]
    [InlineData("1.12345")]
    public void Constructor_InvalidPercentage_Throws(string value)
    {
        // Arrange
        var percentage = decimal.Parse(value, System.Globalization.CultureInfo.InvariantCulture);

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(
            () => CreateFeeConfiguration(percentage: percentage)
        );
    }

    [Theory]
    [InlineData("0")]
    [InlineData("100")]
    [InlineData("1.1234")]
    public void Constructor_ValidPercentageBoundaries_AcceptsValue(string value)
    {
        // Arrange
        var percentage = decimal.Parse(value, System.Globalization.CultureInfo.InvariantCulture);

        // Act
        var fee = CreateFeeConfiguration(percentage: percentage);

        // Assert
        Assert.Equal(percentage, fee.Percentage);
    }

    [Fact]
    public void Constructor_EmptyId_Throws()
    {
        // Arrange
        var id = Guid.Empty;

        // Act & Assert
        Assert.Throws<ArgumentException>(
            () => CreateFeeConfiguration(id: id)
        );
    }

    [Theory]
    [InlineData(0)]
    [InlineData(FeeRules.MaxNameLength + 1)]
    public void Constructor_InvalidName_Throws(int length)
    {
        // Arrange
        var name = new string('N', length);

        // Act & Assert
        Assert.Throws<ArgumentException>(
            () => CreateFeeConfiguration(name: name)
        );
    }

    [Fact]
    public void Constructor_UndefinedType_Throws()
    {
        // Arrange
        var type = (FeeType)999;

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(
            () => CreateFeeConfiguration(type: type)
        );
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Constructor_InvalidPeriod_Throws(bool emptyStart)
    {
        // Arrange
        var start = emptyStart ? default : Start;
        var end = Start.AddDays(-1);

        // Act & Assert
        Assert.Throws<ArgumentException>(
            () => CreateFeeConfiguration(effectiveFrom: start, effectiveTo: end)
        );
    }

    [Fact]
    public void UpdateDetails_InvalidPeriod_ThrowsAndLeavesEveryFieldUnchanged()
    {
        // Arrange
        var name = "Fee";
        var type = FeeType.AdminFee;
        var percentage = 1m;
        var isActive = true;

        var fee = CreateFeeConfiguration(
            name: name,
            type: type,
            percentage: percentage,
            effectiveFrom: Start,
            effectiveTo: null,
            isActive: isActive
        );

        // Act & Assert
        Assert.Throws<ArgumentException>(
            () => fee.UpdateDetails(
                name: "Changed",
                type: FeeType.RiskAdjustment,
                percentage: 50m,
                effectiveFrom: Start,
                effectiveTo: Start.AddDays(-1),
                isActive: false
            )
        );

        Assert.Equal(name, fee.Name);
        Assert.Equal(type, fee.Type);
        Assert.Equal(percentage, fee.Percentage);
        Assert.Equal(Start, fee.EffectiveFrom);
        Assert.Null(fee.EffectiveTo);
        Assert.True(fee.IsActive);
    }

    [Theory]
    [InlineData(-1, null, true, false)]
    [InlineData(0, null, true, true)]
    [InlineData(2, 2, true, true)]
    [InlineData(3, 2, true, false)]
    [InlineData(0, null, false, false)]
    [InlineData(100, null, true, true)]
    public void IsApplicableOn_UsesActivityAndInclusiveDates(
        int dateToCheckOffsetFromStart,
        int? effectiveToOffsetFromStart,
        bool isActive,
        bool expectedIsApplicable
    )
    {
        // Arrange
        DateOnly? effectiveTo = effectiveToOffsetFromStart.HasValue
            ? Start.AddDays(effectiveToOffsetFromStart.Value)
            : null;

        var fee = CreateFeeConfiguration(
            effectiveFrom: Start,
            effectiveTo: effectiveTo,
            isActive: isActive
        );

        var dateToCheck = Start.AddDays(dateToCheckOffsetFromStart);

        // Act
        var actualIsApplicable = fee.IsApplicableOn(dateToCheck);

        // Assert
        Assert.Equal(expectedIsApplicable, actualIsApplicable);
    }

    private static FeeConfiguration CreateFeeConfiguration(
        Guid? id = null,
        string name = "Fee",
        FeeType type = FeeType.AdminFee,
        decimal percentage = 1m,
        DateOnly? effectiveFrom = null,
        DateOnly? effectiveTo = null,
        bool isActive = true
    )
    {
        return new(
            id: id ?? Guid.NewGuid(),
            name: name,
            type: type,
            percentage: percentage,
            effectiveFrom: effectiveFrom ?? Start,
            effectiveTo: effectiveTo,
            isActive: isActive
        );
    }
}
