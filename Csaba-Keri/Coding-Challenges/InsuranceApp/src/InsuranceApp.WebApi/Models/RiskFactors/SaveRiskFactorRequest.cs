using InsuranceApp.WebApi.Models.Buildings;
using System.ComponentModel.DataAnnotations;

namespace InsuranceApp.WebApi.Models.RiskFactors;

public record SaveRiskFactorRequest(
    [Required]
    RiskFactorLevelDto? Level,
    
    Guid? CountryId,
    
    Guid? CountyId,
    
    Guid? CityId,
    
    BuildingTypeDto? BuildingType,
    
    [Required]
    decimal? AdjustmentPercentage,
    
    [Required]
    bool? IsActive
);
