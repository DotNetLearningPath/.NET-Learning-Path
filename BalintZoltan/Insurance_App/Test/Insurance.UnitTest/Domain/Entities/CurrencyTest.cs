using Insurance.Domain.Entities;

namespace Insurance.UnitTest.Domain.Entities;

public sealed class CurrencyTest
{
    [Fact]
    public void Constructor_Should_Set_And_Normalize_Properties()
    {
        // Arrange
        const string code = " usd ";
        const string name = " United States Dollar ";

        // Act
        var currency = new Currency(code, name, 1.25m, false);

        // Assert
        Assert.NotEqual(Guid.Empty, currency.Id);
        Assert.Equal("USD", currency.Code);
        Assert.Equal("United States Dollar", currency.Name);
        Assert.Equal(1.25m, currency.ExchangeRateToBase);
        Assert.False(currency.IsActive);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("US")]
    [InlineData("USDT")]
    public void Constructor_Should_Throw_When_Code_Is_Invalid(string? code)
    {
        // Arrange

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new Currency(code!, "United States Dollar", 1.25m));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void Constructor_Should_Throw_When_Name_Is_Invalid(string? name)
    {
        // Arrange

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new Currency("USD", name!, 1.25m));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-0.01)]
    public void Constructor_Should_Throw_When_Exchange_Rate_Is_Invalid(decimal exchangeRate)
    {
        // Arrange

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Currency("USD", "United States Dollar", exchangeRate));
    }

    [Fact]
    public void Update_Should_Change_And_Normalize_Properties()
    {
        // Arrange
        var currency = new Currency("USD", "United States Dollar", 1.25m);

        // Act
        currency.Update(" eur ", " Euro ", 0.92m);

        // Assert
        Assert.Equal("EUR", currency.Code);
        Assert.Equal("Euro", currency.Name);
        Assert.Equal(0.92m, currency.ExchangeRateToBase);
        Assert.True(currency.IsActive);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Activate_And_Deactivate_Should_Set_Active_State(bool activate)
    {
        // Arrange
        var currency = new Currency("USD", "United States Dollar", 1.25m, !activate);

        // Act
        if (activate)
        {
            currency.Activate();
        }
        else
        {
            currency.Deactivate();
        }

        // Assert
        Assert.Equal(activate, currency.IsActive);
    }
}
