using System.ComponentModel.DataAnnotations;

namespace InsuranceManagement.Application.DTOs.Policy
{
    public class CancelPolicyDto
    {
        [Required]
        [MaxLength(500)]
        public required string Reason { get; set; }

        public DateOnly EffectiveDate { get; set; }
    }
}
