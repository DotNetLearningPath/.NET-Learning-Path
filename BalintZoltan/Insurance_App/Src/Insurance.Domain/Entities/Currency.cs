namespace Insurance.Domain.Entities;

public class Currency
{
    public const int CodeLength = 3;
    public const int NameMaxLength = 100;

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
        Validate(code, name, exchangeRateToBase);

        Id = Guid.NewGuid();
        Code = code.Trim().ToUpperInvariant();
        Name = name.Trim();
        ExchangeRateToBase = exchangeRateToBase;
        IsActive = isActive;
    }

    public void Update(string code, string name, decimal exchangeRateToBase)
    {
        Validate(code, name, exchangeRateToBase);
        Code = code.Trim().ToUpperInvariant();
        Name = name.Trim();
        ExchangeRateToBase = exchangeRateToBase;
    }

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;

    private static void Validate(string code, string name, decimal exchangeRateToBase)
    {
        if (string.IsNullOrWhiteSpace(code) || code.Trim().Length != CodeLength)
        {
            throw new ArgumentException($"Currency code must contain exactly {CodeLength} characters.", nameof(code));
        }
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Currency name is required.", nameof(name));
        }
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(exchangeRateToBase);
    }
}
