using InsuranceApp.Application.Fees;
using InsuranceApp.Application.Policies.Mappings;
using InsuranceApp.Domain.Policies;

namespace InsuranceApp.Application.Policies.Pricing;

public class FeeAdjustmentProvider(IFeeRepository feeRepository) : IPremiumAdjustmentProvider
{
    private readonly IFeeRepository _feeRepository = feeRepository;

    public async Task<IReadOnlyList<AppliedAdjustment>> GetAdjustmentsAsync(PremiumCalculationContext context, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(context);
        cancellationToken.ThrowIfCancellationRequested();
        
        var fees = await _feeRepository.GetApplicableFeesAsync(context.CalculationDate, cancellationToken);
        
        return [.. fees.Select(fee => fee.ToAppliedAdjustment())];
    }
}
