using InsuranceApp.Domain.RiskFactors.Targets;

namespace InsuranceApp.Domain.RiskFactors;

public class RiskFactorConfiguration
{
    public Guid Id { get; }

    public RiskTarget Target { get; private set; }
    public decimal AdjustmentPercentage { get; private set; }
    public bool IsActive { get; private set; }

    public RiskFactorConfiguration(Guid id, RiskTarget target, decimal adjustmentPercentage, bool isActive)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException(
                "Risk factor identifier must not be empty.",
                nameof(id)
            );
        }

        ValidateDetails(target, adjustmentPercentage);

        Id = id;
        Target = target;
        AdjustmentPercentage = adjustmentPercentage;
        IsActive = isActive;
    }

    public void UpdateDetails(RiskTarget target, decimal adjustmentPercentage, bool isActive)
    {
        ValidateDetails(target, adjustmentPercentage);

        Target = target;
        AdjustmentPercentage = adjustmentPercentage;
        IsActive = isActive;
    }

    private static void ValidateDetails(RiskTarget target, decimal adjustmentPercentage)
    {
        ArgumentNullException.ThrowIfNull(target);
        
        if (!RiskFactorRules.IsValidAdjustment(adjustmentPercentage))
        {
            throw new ArgumentOutOfRangeException(
                nameof(adjustmentPercentage),
                adjustmentPercentage,
                $"Adjustment must be between {RiskFactorRules.MinAdjustment} and {RiskFactorRules.MaxAdjustment}" +
                $", with at most {RiskFactorRules.AdjustmentScale} decimal places."
            );
        }
    }
}
