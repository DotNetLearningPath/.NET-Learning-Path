using System.ComponentModel.DataAnnotations;

namespace InsuranceApp.WebApi.Models.Policies;

public record CreateDraftPolicyRequest(
    [Required]
    Guid? ClientId,

    [Required]
    Guid? BuildingId,
    
    [Required]
    Guid? BrokerId,
    
    [Required]
    Guid? CurrencyId,
    
    [Required]
    DateOnly? StartDate,
    
    [Required]
    DateOnly? EndDate,
    
    [Required]
    decimal? BasePremium
);
