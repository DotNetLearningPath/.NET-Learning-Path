namespace InsuranceManagement.Application.DTOs.Geography;

public class UpdateCityDto
{
    public string Name { get; set; } = string.Empty;
    public Guid CountyId { get; set; }
}
