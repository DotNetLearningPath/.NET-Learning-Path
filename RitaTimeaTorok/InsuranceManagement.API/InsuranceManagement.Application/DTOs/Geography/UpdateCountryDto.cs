using System.ComponentModel.DataAnnotations;

namespace InsuranceManagement.Application.DTOs.Geography;

public class UpdateCountryDto
{
    [Required]
    [MaxLength(100)]
    public required string Name { get; set; }
}
