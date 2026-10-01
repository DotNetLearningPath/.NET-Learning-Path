namespace InsuranceApp.Application.Currencies.Commands;

public interface ICurrencyDetailsCommand
{
    string Name { get; }
    decimal ExchangeRateToBase { get; }
    bool IsActive { get; }
}
