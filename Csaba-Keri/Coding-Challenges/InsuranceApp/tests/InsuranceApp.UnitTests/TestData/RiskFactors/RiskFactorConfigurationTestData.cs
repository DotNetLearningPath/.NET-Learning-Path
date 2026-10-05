using InsuranceApp.Domain.RiskFactors;
using InsuranceApp.Domain.RiskFactors.Targets;

namespace InsuranceApp.UnitTests.TestData.RiskFactors;

internal static class RiskFactorConfigurationTestData
{
    public static readonly Guid DefaultId =
        Guid.Parse("44444444-4444-4444-4444-444444444444");

    public static readonly RiskTarget DefaultRiskTarget =
        RiskTargetTestData.CreateCountryTarget();

    public const decimal DefaultAdjustmentPercentage = 5m;

    public const bool DefaultIsActive = true;

    public static RiskFactorConfiguration Create(
        Guid? id = null,
        RiskTarget? target = null,
        decimal adjustmentPercentage = DefaultAdjustmentPercentage,
        bool isActive = DefaultIsActive
    )
    {
        return new(
            id: id ?? DefaultId,
            target: target ?? DefaultRiskTarget,
            adjustmentPercentage: adjustmentPercentage,
            isActive: isActive
        );
    }
}
