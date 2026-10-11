using InsuranceApp.Domain.Policies;

namespace InsuranceApp.Application.Policies.Pricing;

public record PremiumCalculationResult(
    decimal FinalPremium,
    IReadOnlyList<AppliedAdjustment> Adjustments
);
