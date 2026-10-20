namespace Insurance.Application.DTO.Currencies;

public sealed class CurrencyDto
{
    public Guid Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public decimal ExchangeRateToBase { get; init; }
    public bool IsActive { get; init; }
}
