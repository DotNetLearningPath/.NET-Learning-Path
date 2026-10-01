using Insurance.Domain.Enums;
using Insurance.Application.Abstractions;

namespace Insurance.Application.Services;

public sealed class PremiumCalculationService : IPremiumCalculationService
{
    private readonly IFeeConfigurationRepository _feeConfigurationRepository;

    public PremiumCalculationService(
        IFeeConfigurationRepository feeConfigurationRepository,
        IRiskFactorRepository? riskFactorRepository = null)
    {
        _feeConfigurationRepository = feeConfigurationRepository;
    }

    public async Task<decimal> CalculateFinalPremiumAsync(
        decimal basePremium,
        DateTime effectiveAt,
        CancellationToken cancellationToken)
    {
        if (basePremium < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(basePremium));
        }

        var configurations = await _feeConfigurationRepository
            .GetActiveFeeConfigurationsAsync(effectiveAt, cancellationToken);

        var percentageTotal = configurations
            .Where(configuration => configuration.Type == FeeType.Percentage)
            .Sum(configuration => configuration.Percentage);

        var fixedAmountTotal = configurations
            .Where(configuration => configuration.Type == FeeType.FixedAmount)
            .Sum(configuration => configuration.Percentage);

        return basePremium * (1 + percentageTotal / 100m) + fixedAmountTotal;
    }
}
