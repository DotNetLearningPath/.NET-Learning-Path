using Insurance.Application.Abstractions;
using Insurance.Application.DTO.Premiums;

namespace Insurance.Application.Services;

public sealed class PremiumCalculationService(
    IFeeConfigurationRepository feeConfigurationRepository,
        IRiskFactorRepository riskFactorRepository) : IPremiumCalculationService
{
    public async Task<decimal> CalculateFinalPremiumAsync(
        decimal basePremium,
        DateTime effectiveAt,
        PremiumCalculationContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfNegative<decimal>(basePremium);
        ArgumentNullException.ThrowIfNull(context);

        var feeConfigurations = await feeConfigurationRepository
            .GetActiveFeeConfigurationsAsync(effectiveAt, cancellationToken);

        var riskFactors = await riskFactorRepository.GetApplicableRiskFactorsAsync(
            context.CountryId,
            context.CountyId,
            context.CityId,
            context.BuildingType,
            cancellationToken);

        var feePercentageTotal = feeConfigurations
            .Sum(configuration => configuration.Percentage);
        var riskAdjustmentTotal = riskFactors
            .Sum(configuration => configuration.AdjustmentPercentage);
        var totalPercentage = feePercentageTotal + riskAdjustmentTotal;

        return basePremium * (1 + totalPercentage / 100m);
    }
}
