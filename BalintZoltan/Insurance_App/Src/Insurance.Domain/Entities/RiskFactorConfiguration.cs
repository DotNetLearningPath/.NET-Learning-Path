using Domain.Enums;

namespace Domain.Entities;

public class RiskFactorConfiguration
{
    public Guid Id { get; private set; }
    public RiskFactorLevel Level { get; private set; }
    public string Reference { get; private set; }
    public decimal AdjustmentPercentage { get; private set; }
    public bool IsActive { get; private set; }

    private RiskFactorConfiguration()
    {
        Reference = null!;
    }

    public RiskFactorConfiguration(
        RiskFactorLevel level,
        string reference,
        decimal adjustmentPercentage,
        bool isActive = true)
    {
        if (!Enum.IsDefined(level)) throw new ArgumentException("Risk factor level is not valid.", nameof(level));
        if (string.IsNullOrWhiteSpace(reference))
            throw new ArgumentException("Reference is required.", nameof(reference));

        Id = Guid.NewGuid();
        Level = level;
        Reference = reference;
        AdjustmentPercentage = adjustmentPercentage;
        IsActive = isActive;
    }
}
