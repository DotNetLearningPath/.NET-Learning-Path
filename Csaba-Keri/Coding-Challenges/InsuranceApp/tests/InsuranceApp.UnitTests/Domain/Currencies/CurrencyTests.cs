using InsuranceApp.Domain.Currencies;

namespace InsuranceApp.UnitTests.Domain.Currencies;

public sealed class CurrencyTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Constructor_ValidValues_NormalizesCodeAndName(bool isActive)
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var currency = CreateCurrency(
            id: id,
            code: " eur ",
            name: " Euro ",
            exchangeRateToBase: 5.12345678m,
            isActive: isActive
        );

        // Assert
        Assert.Equal(id, currency.Id);
        Assert.Equal("EUR", currency.Code);
        Assert.Equal("Euro", currency.Name);
        Assert.Equal(5.12345678m, currency.ExchangeRateToBase);
        Assert.Equal(isActive, currency.IsActive);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("EU")]
    [InlineData("EURO")]
    [InlineData("EU1")]
    [InlineData("ÉUR")]
    public void Constructor_InvalidCode_Throws(string? code)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(
            () => CreateCurrency(
                code: code!,
                name: "Euro",
                exchangeRateToBase: 5.28m,
                isActive: true
            )
        );
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("0.000000001")]
    [InlineData("1.123456789")]
    [InlineData("10000000000")]
    public void Constructor_InvalidRate_Throws(string value)
    {
        // Arrange
        var rate = decimal.Parse(value, System.Globalization.CultureInfo.InvariantCulture);

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(
            () => CreateCurrency(exchangeRateToBase: rate)
        );
    }

    [Theory]
    [InlineData("0.00000001")]
    [InlineData("9999999999.99999999")]
    public void Constructor_BoundaryRate_AcceptsWithoutRounding(string value)
    {
        // Arrange
        var rate = decimal.Parse(value, System.Globalization.CultureInfo.InvariantCulture);

        // Act
        var currency = CreateCurrency(code: "XYZ", exchangeRateToBase: rate);

        // Assert
        Assert.Equal(rate, currency.ExchangeRateToBase);
    }

    [Fact]
    public void Constructor_EmptyId_Throws()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(
            () => CreateCurrency(id: Guid.Empty)
        );
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public void Constructor_InvalidName_Throws(int nameLength)
    {
        // Arrange
        var name = new string('N', nameLength);

        // Act & Assert
        Assert.Throws<ArgumentException>(
            () => CreateCurrency(name: name)
        );
    }

    [Fact]
    public void Constructor_BaseCurrencyWithDifferentRate_Throws()
    {
        // Arrange
        const decimal rate = 2m;

        // Act & Assert
        Assert.Throws<ArgumentException>(
            () => CreateCurrency(
                code: CurrencyRules.BaseCurrencyCode,
                exchangeRateToBase: rate
            )
        );
    }

    [Fact]
    public void UpdateDetails_ValidValues_PreservesIdentityAndCanDeactivate()
    {
        // Arrange
        var currency = CreateCurrency(
            code: "EUR",
            name: "Euro",
            exchangeRateToBase: 5m,
            isActive: true
        );
        
        var currencyId = currency.Id;

        // Act
        currency.UpdateDetails(
            name: " Updated Euro ",
            exchangeRateToBase: 5.25m,
            isActive: false
        );

        // Assert
        Assert.Equal(currencyId, currency.Id);
        Assert.Equal("EUR", currency.Code);
        Assert.Equal("Updated Euro", currency.Name);
        Assert.Equal(5.25m, currency.ExchangeRateToBase);
        Assert.False(currency.IsActive);
    }

    [Fact]
    public void UpdateDetails_BaseCurrencyWithDifferentRate_LeavesAllFieldsUnchanged()
    {
        // Arrange
        var currency = CreateCurrency(
            code: CurrencyRules.BaseCurrencyCode,
            name: "Base Currency",
            exchangeRateToBase: 1m,
            isActive: true
        );

        // Act & Assert
        Assert.Throws<ArgumentException>(
            () => currency.UpdateDetails(
                name: "Changed",
                exchangeRateToBase: 2m,
                isActive: false
            )
        );
        
        Assert.Equal("Base Currency", currency.Name);
        Assert.Equal(1m, currency.ExchangeRateToBase);
        Assert.True(currency.IsActive);
    }

    private static Currency CreateCurrency(
        Guid? id = null,
        string code = "RON",
        string name = "Romanian Leu",
        decimal exchangeRateToBase = 1m,
        bool isActive = true
    )
    {
        return new(
            id: id ?? Guid.NewGuid(),
            code: code,
            name: name,
            exchangeRateToBase: exchangeRateToBase,
            isActive: isActive
        );
    }
}
