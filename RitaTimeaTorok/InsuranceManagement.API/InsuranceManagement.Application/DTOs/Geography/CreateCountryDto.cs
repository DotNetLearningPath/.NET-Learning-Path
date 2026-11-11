using System.ComponentModel.DataAnnotations;

namespace InsuranceManagement.Application.DTOs.Geography
{
    public class CreateCountryDto
    {
        [Required]
        [MaxLength(100)]
        public required string Name { get; set; }
    }
}
