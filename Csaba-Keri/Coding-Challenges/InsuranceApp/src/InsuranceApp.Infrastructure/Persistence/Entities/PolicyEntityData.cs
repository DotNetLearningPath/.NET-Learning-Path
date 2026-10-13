using InsuranceApp.Domain.Policies;

namespace InsuranceApp.Infrastructure.Persistence.Entities;

public record PolicyEntityData
{
    public required Guid PolicyId { get; init; }
    public required string PolicyNumber { get; init; }
    public required PolicyStatus Status { get; init; }
    public required Guid ClientId { get; init; }
    public required Guid BuildingId { get; init; }
    public required Guid BrokerId { get; init; }
    public required Guid CurrencyId { get; init; }
    public required decimal CurrencyExchangeRateToBase { get; init; }
    public required DateOnly StartDate { get; init; }
    public required DateOnly EndDate { get; init; }
    public required decimal BasePremium { get; init; }
    public required decimal FinalPremium { get; init; }
    public required ICollection<PolicyAdjustmentEntity> AppliedAdjustments { get; init; }
    public required DateTimeOffset CreatedAt { get; init; }
    public required DateTimeOffset UpdatedAt { get; init; }
}
