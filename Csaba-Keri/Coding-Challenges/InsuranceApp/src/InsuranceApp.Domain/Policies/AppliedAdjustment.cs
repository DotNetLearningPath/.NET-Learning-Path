using InsuranceApp.Domain.Fees;
using InsuranceApp.Domain.RiskFactors;

namespace InsuranceApp.Domain.Policies;

public record AppliedAdjustment
{
    public AdjustmentSource Source { get; }
    public Guid ConfigurationId { get; }
    public decimal Percentage { get; }

    public AppliedAdjustment(AdjustmentSource source, Guid configurationId, decimal percentage)
    {
        if (!Enum.IsDefined(source))
        {
            throw new ArgumentOutOfRangeException(
                nameof(source),
                source,
                "Adjustment source is invalid."
            );
        }
        
        if (configurationId == Guid.Empty)
        {
            throw new ArgumentException(
                "Configuration identifier must not be empty.",
                nameof(configurationId)
            );
        }

        var isValidPercentage = source switch
        {
            AdjustmentSource.Fee => FeeRules.IsValidPercentage(percentage),
            AdjustmentSource.RiskFactor => RiskFactorRules.IsValidAdjustment(percentage),

            _ => throw new ArgumentOutOfRangeException(
                nameof(source),
                source,
                "Adjustment source is not supported."
            )
        };

        if (!isValidPercentage)
        {
            throw new ArgumentOutOfRangeException(
                nameof(percentage),
                percentage,
                "Percentage is invalid for the selected adjustment source."
            );
        }
        
        Source = source;
        ConfigurationId = configurationId;
        Percentage = percentage;
    }
}
