using InsuranceApp.Application.Common;
using InsuranceApp.Domain.Entities;

namespace InsuranceApp.Application.Abstractions.Services;

public interface IPolicyCalculationService
{
    Task<Result<decimal>> CalculateFinalPremiumAsync(
        decimal basePremium, 
        Building building, 
        DateTime policyStartDate,
        CancellationToken cancellationToken
    );
}
