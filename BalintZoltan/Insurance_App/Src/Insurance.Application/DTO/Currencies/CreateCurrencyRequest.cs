namespace Insurance.Application.DTO.Currencies;

public sealed class CreateCurrencyRequest
{
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public decimal ExchangeRateToBase { get; init; } = 1m;
    public bool IsActive { get; init; } = true;
}
