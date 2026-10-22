using InsuranceApp.Application.Fees.Results;
using InsuranceApp.Domain.Fees;

namespace InsuranceApp.Application.Fees.Mappings;

internal static class FeeMappings
{
    public static FeeResult ToResult(this FeeConfiguration fee)
    {
        return new(
            Id: fee.Id,
            Name: fee.Name,
            Type: fee.Type,
            Percentage: fee.Percentage,
            EffectiveFrom: fee.EffectiveFrom,
            EffectiveTo: fee.EffectiveTo,
            IsActive: fee.IsActive
        );
    }
}
