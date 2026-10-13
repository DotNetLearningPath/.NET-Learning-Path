namespace InsuranceApp.Application.Policies.Pricing;

public interface IPolicyPremiumCalculator
{
    Task<PremiumCalculationResult> CalculatePremiumAsync(decimal basePremium, PremiumCalculationContext context, CancellationToken cancellationToken);
}
