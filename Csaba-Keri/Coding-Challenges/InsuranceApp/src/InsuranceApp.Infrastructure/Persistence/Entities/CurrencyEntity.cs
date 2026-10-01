namespace InsuranceApp.Infrastructure.Persistence.Entities;

public class CurrencyEntity(
    Guid id,
    string code,
    string name,
    decimal exchangeRateToBase,
    bool isActive
)
{
    public Guid Id { get; private set; } = id;
    public string Code { get; private set; } = code;
    public string Name { get; private set; } = name;
    public decimal ExchangeRateToBase { get; private set; } = exchangeRateToBase;
    public bool IsActive { get; private set; } = isActive;
}
