namespace Application.Abstractions;

public interface IPremiumCalculationService
{
    Task<decimal> CalculateFinalPremiumAsync(
        decimal basePremium,
        DateTime effectiveAt,
        CancellationToken cancellationToken = default);
}
