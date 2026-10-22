using System.ComponentModel.DataAnnotations;

namespace InsuranceApp.WebApi.Models.Fees;

public record SaveFeeRequest(
    [Required]
    string Name,
    
    [Required]
    FeeTypeDto? Type,
    
    [Required]
    decimal? Percentage,
    
    [Required]
    DateOnly? EffectiveFrom,
    
    DateOnly? EffectiveTo,
    
    [Required]
    bool? IsActive
);
