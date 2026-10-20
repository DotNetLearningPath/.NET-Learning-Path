using InsuranceApp.Domain.Policies;

namespace InsuranceApp.Infrastructure.Persistence.Entities;

public class PolicyEntity
{
    public Guid Id { get; private set; }
    public string PolicyNumber { get; private set; }
    public PolicyStatus Status { get; private set; }
    public Guid ClientId { get; private set; }
    public Guid BuildingId { get; private set; }
    public Guid BrokerId { get; private set; }
    public Guid CurrencyId { get; private set; }
    public decimal CurrencyExchangeRateToBase { get; private set; }
    public DateOnly StartDate { get; private set; }
    public DateOnly EndDate { get; private set; }
    public decimal BasePremium { get; private set; }
    public decimal FinalPremium { get; private set; }
    public ICollection<PolicyAdjustmentEntity> AppliedAdjustments { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public DateTimeOffset? ActivatedAt { get; private set; }
    public DateTimeOffset? CancelledAt { get; private set; }
    public DateOnly? CancellationEffectiveDate { get; private set; }
    public string? CancellationReason { get; private set; }
    public DateTimeOffset? ExpiredAt { get; private set; }

    private PolicyEntity()
    {
        // Used by EF Core.
        PolicyNumber = string.Empty;
        AppliedAdjustments = [];
    }

    public PolicyEntity(PolicyEntityData data)
    {
        ArgumentNullException.ThrowIfNull(data);

        Id = data.PolicyId;
        PolicyNumber = data.PolicyNumber;
        Status = data.Status;
        ClientId = data.ClientId;
        BuildingId = data.BuildingId;
        BrokerId = data.BrokerId;
        CurrencyId = data.CurrencyId;
        CurrencyExchangeRateToBase = data.CurrencyExchangeRateToBase;
        StartDate = data.StartDate;
        EndDate = data.EndDate;
        BasePremium = data.BasePremium;
        FinalPremium = data.FinalPremium;
        AppliedAdjustments = data.AppliedAdjustments;
        CreatedAt = data.CreatedAt;
        UpdatedAt = data.UpdatedAt;
        ActivatedAt = data.ActivatedAt;
        CancelledAt = data.CancelledAt;
        CancellationEffectiveDate = data.CancellationEffectiveDate;
        CancellationReason = data.CancellationReason;
        ExpiredAt = data.ExpiredAt;
    }
}
