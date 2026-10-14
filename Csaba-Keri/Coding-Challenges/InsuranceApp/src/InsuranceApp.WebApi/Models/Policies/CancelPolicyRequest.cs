using System.ComponentModel.DataAnnotations;

namespace InsuranceApp.WebApi.Models.Policies;

public record CancelPolicyRequest(
    [Required]
    string Reason,

    [Required]
    DateOnly? EffectiveDate
);
