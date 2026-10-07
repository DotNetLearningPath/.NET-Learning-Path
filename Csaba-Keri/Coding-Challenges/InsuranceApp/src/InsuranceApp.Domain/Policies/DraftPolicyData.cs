namespace InsuranceApp.Domain.Policies;

public record DraftPolicyData
{
    public required Guid PolicyId { get; init; }
    public required Guid ClientId { get; init; }
    public required Guid BuildingId { get; init; }
    public required Guid BrokerId { get; init; }
    public required Guid CurrencyId { get; init; }
    public required decimal CurrencyExchangeRateToBase { get; init; }
    public required DateOnly StartDate { get; init; }
    public required DateOnly EndDate { get; init; }
    public required decimal BasePremium { get; init; }
    public required decimal FinalPremium { get; init; }
    public required IEnumerable<AppliedAdjustment> AppliedAdjustments { get; init; }
}
