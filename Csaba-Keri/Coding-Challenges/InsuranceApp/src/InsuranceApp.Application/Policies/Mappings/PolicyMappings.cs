using InsuranceApp.Application.Policies.Results;
using InsuranceApp.Domain.Policies;

namespace InsuranceApp.Application.Policies.Mappings;

internal static class PolicyMappings
{
    public static PolicyResult ToResult(this Policy policy, string currencyCode)
    {
        return new(
            Id: policy.Id,
            PolicyNumber: policy.PolicyNumber,
            Status: policy.Status,
            ClientId: policy.ClientId,
            BuildingId: policy.BuildingId,
            BrokerId: policy.BrokerId,
            CurrencyId: policy.CurrencyId,
            CurrencyCode: currencyCode,
            CurrencyExchangeRateToBase: policy.CurrencyExchangeRateToBase,
            StartDate: policy.StartDate,
            EndDate: policy.EndDate,
            BasePremium: policy.BasePremium,
            FinalPremium: policy.FinalPremium,
            AppliedAdjustments: policy.AppliedAdjustments,
            CreatedAt: policy.CreatedAt,
            UpdatedAt: policy.UpdatedAt,
            ActivatedAt: policy.ActivatedAt,
            CancelledAt: policy.CancelledAt,
            CancellationEffectiveDate: policy.CancellationEffectiveDate,
            CancellationReason: policy.CancellationReason,
            ExpiredAt: policy.ExpiredAt
        );
    }
}
