using InsuranceApp.WebApi.Models.Buildings;
using InsuranceApp.WebApi.Models.Policies;
using System.ComponentModel.DataAnnotations;

namespace InsuranceApp.WebApi.Models.Reports;

public record PolicyReportRequest(
    [Required]
    DateOnly? From,
    
    [Required]
    DateOnly? To,
    
    PolicyStatusDto? Status = null,
    
    Guid? CurrencyId = null,

    BuildingTypeDto? BuildingType = null
);
