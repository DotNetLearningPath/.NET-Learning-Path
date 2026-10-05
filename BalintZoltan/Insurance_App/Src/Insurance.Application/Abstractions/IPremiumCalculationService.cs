using Insurance.Application.DTO.Premiums;

namespace Insurance.Application.Abstractions;

public interface IPremiumCalculationService
{
    Task<decimal> CalculateFinalPremiumAsync(
        decimal basePremium,
        DateTime effectiveAt,
        PremiumCalculationContext context,
        CancellationToken cancellationToken = default);
}
