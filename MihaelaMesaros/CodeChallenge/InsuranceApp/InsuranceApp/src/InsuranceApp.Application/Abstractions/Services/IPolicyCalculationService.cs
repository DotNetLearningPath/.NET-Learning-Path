namespace InsuranceApp.Application.Abstractions.Services;

public interface IPolicyCalculationService
{
    Task<decimal> CalculateFinalPremiumAsync(decimal basePremium, Guid buildingId, CancellationToken cancellationToken);
}
