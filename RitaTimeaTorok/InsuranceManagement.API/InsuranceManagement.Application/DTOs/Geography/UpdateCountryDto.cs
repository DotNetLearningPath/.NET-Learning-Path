using System.ComponentModel.DataAnnotations;

namespace InsuranceManagement.Application.DTOs.Geography;

public class UpdateCountryDto
{
    [MaxLength(100)]
    public required string Name { get; set; }
}
