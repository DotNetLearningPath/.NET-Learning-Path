namespace InsuranceManagement.Application.DTOs.Geography;

public class CreateCityDto
{
    public string Name { get; set; } = string.Empty;
    public Guid CountyId { get; set; }
}
