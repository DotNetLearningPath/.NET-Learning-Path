using System.ComponentModel.DataAnnotations;

namespace InsuranceManagement.Application.DTOs.Geography;

public class CreateCountyDto
{
    [Required]
    [MaxLength(100)]
    public required string Name { get; set; }
    public required Guid CountryId { get; set; }
}
