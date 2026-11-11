using System.ComponentModel.DataAnnotations;

namespace InsuranceManagement.Application.DTOs.Metadata
{
    public class CreateFeeConfigurationDto
    {
        [Required]
        [MaxLength(200)]
        public required string Name { get; set; }

        [Required]
        [MaxLength(50)]
        public required string Type { get; set; }

        [Range(typeof(decimal), "0", "300")]
        public required decimal Percentage { get; set; }

        public required DateOnly EffectiveFrom { get; set; }
        public DateOnly? EffectiveTo { get; set; }
        public required bool IsActive { get; set; }
    }
}
