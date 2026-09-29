namespace Domain.Entities;

public class Currency
{
    public Guid Id { get; private set; }
    public string Code { get; private set; }
    public string Name { get; private set; }
    public decimal ExchangeRateToBase { get; private set; }
    public bool IsActive { get; private set; }

    private Currency()
    {
        Code = null!;
        Name = null!;
    }

    public Currency(string code, string name, decimal exchangeRateToBase, bool isActive = true)
    {
        if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Currency code is required.", nameof(code));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Currency name is required.", nameof(name));
        if (exchangeRateToBase <= 0) throw new ArgumentOutOfRangeException(nameof(exchangeRateToBase));

        Id = Guid.NewGuid();
        Code = code;
        Name = name;
        ExchangeRateToBase = exchangeRateToBase;
        IsActive = isActive;
    }
}
