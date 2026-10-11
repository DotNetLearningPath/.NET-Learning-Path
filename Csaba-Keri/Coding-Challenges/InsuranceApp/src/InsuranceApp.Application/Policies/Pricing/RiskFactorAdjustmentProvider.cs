using InsuranceApp.Application.Policies.Mappings;
using InsuranceApp.Application.RiskFactors;
using InsuranceApp.Domain.Policies;

namespace InsuranceApp.Application.Policies.Pricing;

public class RiskFactorAdjustmentProvider(IRiskFactorRepository riskFactorRepository) : IPremiumAdjustmentProvider
{
    private readonly IRiskFactorRepository _riskFactorRepository = riskFactorRepository;

    public async Task<IReadOnlyList<AppliedAdjustment>> GetAdjustmentsAsync(PremiumCalculationContext context, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(context);
        cancellationToken.ThrowIfCancellationRequested();

        var riskFactors = await _riskFactorRepository.GetApplicableRiskFactorsAsync(context.CityId, context.BuildingType, cancellationToken);
        
        return [.. riskFactors.Select(riskFactor => riskFactor.ToAppliedAdjustment())];
    }
}
