using InsuranceApp.Domain.Fees;
using InsuranceApp.Domain.Policies;
using InsuranceApp.Domain.RiskFactors;

namespace InsuranceApp.Application.Policies.Mappings;

internal static class AppliedAdjustmentMappings
{
    public static AppliedAdjustment ToAppliedAdjustment(this FeeConfiguration fee)
    {
        return new(
            source: AdjustmentSource.Fee,
            configurationId: fee.Id,
            percentage: fee.Percentage
        );
    }

    public static AppliedAdjustment ToAppliedAdjustment(this RiskFactorConfiguration riskFactor)
    {
        return new(
            source: AdjustmentSource.RiskFactor,
            configurationId: riskFactor.Id,
            percentage: riskFactor.AdjustmentPercentage
        );
    }
}
