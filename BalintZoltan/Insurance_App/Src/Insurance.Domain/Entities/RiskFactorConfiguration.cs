using Insurance.Domain.Enums;

namespace Insurance.Domain.Entities;

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
        Validate(level, reference);

        Id = Guid.NewGuid();
        Level = level;
        Reference = reference;
        AdjustmentPercentage = adjustmentPercentage;
        IsActive = isActive;
    }

    public void Update(
        RiskFactorLevel level,
        string reference,
        decimal adjustmentPercentage)
    {
        Validate(level, reference);
        Level = level;
        Reference = reference;
        AdjustmentPercentage = adjustmentPercentage;
    }

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;

    private static void Validate(RiskFactorLevel level, string reference)
    {
        if (!Enum.IsDefined(level))
        {
            throw new ArgumentException("Risk factor level is not valid.", nameof(level));
        }
        if (string.IsNullOrWhiteSpace(reference))
        {
            throw new ArgumentException("Reference is required.", nameof(reference));
        }
    }
}
