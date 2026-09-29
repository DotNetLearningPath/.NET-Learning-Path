using Domain.Enums;

namespace Domain.Entities;

public class Policy
{
    public Guid Id { get; private set; }
    public string PolicyNumber { get; private set; }
    public Guid ClientId { get; private set; }
    public Guid BuildingId { get; private set; }
    public Guid BrokerId { get; private set; }
    public Guid CurrencyId { get; private set; }
    public PolicyStatus Status { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public decimal BasePremium { get; private set; }
    public decimal FinalPremium { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime? CancellationDate { get; private set; }
    public string? CancellationReason { get; private set; }

    private Policy()
    {
        PolicyNumber = null!;
    }

    public Policy(string policyNumber, Guid clientId, Guid buildingId, Guid brokerId,
        Guid currencyId, DateTime startDate, DateTime endDate, decimal basePremium,
        decimal finalPremium, PolicyStatus status = PolicyStatus.Draft)
    {
        if (string.IsNullOrWhiteSpace(policyNumber)) throw new ArgumentException("Policy number is required.", nameof(policyNumber));
        if (clientId == Guid.Empty) throw new ArgumentException("Client is required.", nameof(clientId));
        if (buildingId == Guid.Empty) throw new ArgumentException("Building is required.", nameof(buildingId));
        if (brokerId == Guid.Empty) throw new ArgumentException("Broker is required.", nameof(brokerId));
        if (currencyId == Guid.Empty) throw new ArgumentException("Currency is required.", nameof(currencyId));
        if (endDate <= startDate) throw new ArgumentException("Policy end date must be after start date.", nameof(endDate));
        if (basePremium < 0) throw new ArgumentOutOfRangeException(nameof(basePremium));
        if (finalPremium < 0) throw new ArgumentOutOfRangeException(nameof(finalPremium));
        if (!Enum.IsDefined(status)) throw new ArgumentException("Policy status is not valid.", nameof(status));

        Id = Guid.NewGuid();
        PolicyNumber = policyNumber;
        ClientId = clientId;
        BuildingId = buildingId;
        BrokerId = brokerId;
        CurrencyId = currencyId;
        Status = status;
        StartDate = startDate;
        EndDate = endDate;
        BasePremium = basePremium;
        FinalPremium = finalPremium;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
    }

    public void Cancel(string reason, DateTime cancellationDate)
    {
        if (Status != PolicyStatus.Active)
            throw new InvalidOperationException("Only active policies can be cancelled.");
        if (string.IsNullOrWhiteSpace(reason)) throw new ArgumentException("Cancellation reason is required.", nameof(reason));
        Status = PolicyStatus.Cancelled;
        CancellationDate = cancellationDate;
        CancellationReason = reason;
        UpdatedAt = DateTime.UtcNow;
    }
}
