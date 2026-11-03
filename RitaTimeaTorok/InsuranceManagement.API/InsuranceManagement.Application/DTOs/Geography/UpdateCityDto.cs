using System.ComponentModel.DataAnnotations;

namespace InsuranceManagement.Application.DTOs.Geography;

public class UpdateCityDto
{
    [MaxLength(100)]
    public required string Name { get; set; }
    public required Guid CountyId { get; set; }
}
