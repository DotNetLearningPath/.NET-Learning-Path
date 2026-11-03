using System.ComponentModel.DataAnnotations;

namespace InsuranceManagement.Application.DTOs.Geography;

public class CreateCountryDto
{
    [MaxLength(100)]
    public required string Name { get; set; }
}
