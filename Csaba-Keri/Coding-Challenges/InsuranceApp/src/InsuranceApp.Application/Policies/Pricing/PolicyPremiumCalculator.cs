using InsuranceApp.Domain.Policies;

namespace InsuranceApp.Application.Policies.Pricing;

public class PolicyPremiumCalculator(IEnumerable<IPremiumAdjustmentProvider> adjustmentProviders) : IPolicyPremiumCalculator
{
    private readonly IEnumerable<IPremiumAdjustmentProvider> _adjustmentProviders = adjustmentProviders;

    public async Task<PremiumCalculationResult> CalculatePremiumAsync(decimal basePremium, PremiumCalculationContext context, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(context);
        cancellationToken.ThrowIfCancellationRequested();

        var adjustments = new List<AppliedAdjustment>();

        foreach (var provider in _adjustmentProviders)
        {
            adjustments.AddRange(await provider.GetAdjustmentsAsync(context, cancellationToken));
        }

        var finalPremium = PolicyPricing.CalculateFinalPremium(basePremium, adjustments);
        
        return new(
            FinalPremium: finalPremium,
            Adjustments: adjustments.AsReadOnly()
        );
    }
}
