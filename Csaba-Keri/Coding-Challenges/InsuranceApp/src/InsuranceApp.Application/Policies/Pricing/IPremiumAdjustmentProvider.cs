using InsuranceApp.Domain.Policies;

namespace InsuranceApp.Application.Policies.Pricing;

public interface IPremiumAdjustmentProvider
{
    Task<IReadOnlyList<AppliedAdjustment>> GetAdjustmentsAsync(PremiumCalculationContext context, CancellationToken cancellationToken);
}
