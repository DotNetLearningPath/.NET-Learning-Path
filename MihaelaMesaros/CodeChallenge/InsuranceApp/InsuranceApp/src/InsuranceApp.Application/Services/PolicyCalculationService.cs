using InsuranceApp.Application.Abstractions.Persistence;
using InsuranceApp.Application.Abstractions.Services;
using InsuranceApp.Application.Common;
using InsuranceApp.Domain.Entities;

namespace InsuranceApp.Application.Services;

public sealed class PolicyCalculationService(
    IFeeConfigRepository feeConfigRepository, 
    IRiskFactorConfigRepository riskFactorConfigRepository, 
    IGeographyRepository geographyRepository
    ) : IPolicyCalculationService
{
    public async Task<Result<decimal>> CalculateFinalPremiumAsync(
        decimal basePremium, 
        Building building, 
        DateTime policyStartDate,
        CancellationToken cancellationToken
    )
    {
        var geographyDetails = await geographyRepository.GetGeographyDetailsByCityAsync(building.CityId, cancellationToken);

        if (geographyDetails is null)
        {
            return Result<decimal>.Failure(GeographyErrors.CityNotFound(building.CityId));
        }

        var feeConfigs = await feeConfigRepository.GetActiveFeeConfigsAsync(policyStartDate, null, cancellationToken);
        var feePercentage = feeConfigs.Sum(x => x.Percentage);

        var riskFactorConfigs = await riskFactorConfigRepository.GetActiveRiskFactorConfigsAsync(
            geographyDetails.CountryId, 
            geographyDetails.CountyId, 
            geographyDetails.CityId, 
            building.BuildingTypeId, 
            cancellationToken);
        
        var riskPercentage = riskFactorConfigs.Sum(x => x.AdjustmentPercentage);

        var totalPercentage = feePercentage + riskPercentage;

        var finalPremium = decimal.Round(basePremium * (1 + totalPercentage / 100m), 2);

        return Result<decimal>.Success(finalPremium);
    }
}