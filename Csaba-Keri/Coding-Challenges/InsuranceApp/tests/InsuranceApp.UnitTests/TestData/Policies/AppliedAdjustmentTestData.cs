using InsuranceApp.Domain.Fees;
using InsuranceApp.Domain.Policies;
using InsuranceApp.Domain.RiskFactors;
using InsuranceApp.UnitTests.TestData.RiskFactors;

namespace InsuranceApp.UnitTests.TestData.Policies;

internal static class AppliedAdjustmentTestData
{
    public const AdjustmentSource DefaultSource = AdjustmentSource.RiskFactor;
    public const decimal DefaultPercentage = RiskFactorConfigurationTestData.DefaultAdjustmentPercentage;
    public static readonly Guid DefaultConfigurationId = RiskFactorConfigurationTestData.DefaultId;

    public static TheoryData<AdjustmentSource, decimal> ValidPercentagesBySource => new()
    {
        { AdjustmentSource.Fee, FeeRules.MinPercentage },
        { AdjustmentSource.Fee, FeeRules.MaxPercentage },

        { AdjustmentSource.RiskFactor, RiskFactorRules.MinAdjustment },
        { AdjustmentSource.RiskFactor, RiskFactorRules.MaxAdjustment }
    };

    public static TheoryData<AdjustmentSource, decimal> InvalidPercentagesBySource => new()
    {
        { AdjustmentSource.Fee, FeeRules.MinPercentage - 0.01m },
        { AdjustmentSource.Fee, FeeRules.MaxPercentage + 0.01m },

        { AdjustmentSource.RiskFactor, RiskFactorRules.MinAdjustment - 0.01m },
        { AdjustmentSource.RiskFactor, RiskFactorRules.MaxAdjustment + 0.01m }
    };

    public static AppliedAdjustment Create(
        AdjustmentSource source = DefaultSource,
        Guid? configurationId = null,
        decimal percentage = DefaultPercentage
    )
    {
        return new(
            source: source,
            configurationId: configurationId ?? DefaultConfigurationId,
            percentage: percentage
        );
    }
}
